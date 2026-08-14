using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying;
using Yingmod.Ying.Helpers;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Patch;

/// <summary>
/// 在战斗UI激活时创建每个玩家的心跳节点（仅本地Ying可见）。
/// </summary>
[HarmonyPatch(typeof(NCombatUi), "Activate")]
public static class HeartbeatNodePatch
{
    private static readonly Dictionary<ulong, Control> _heartbeatNodes = new();

    public static void Postfix(NCombatUi __instance, CombatState state)
    {
        if (!GodotObject.IsInstanceValid(__instance)) return;

        foreach (var oldNode in _heartbeatNodes.Values.ToList())
        {
            if (oldNode != null && GodotObject.IsInstanceValid(oldNode))
                oldNode.QueueFree();
        }
        _heartbeatNodes.Clear();

        var player = state.Players.OfType<Player>().FirstOrDefault(LocalContext.IsMe);
        if (!YingCharacterScope.IsYing(player)) return;

        var prefab = GD.Load<PackedScene>("res://Ying/Scenes/HeartbeatNode.tscn");
        if (prefab == null) return;

        var node = prefab.Instantiate<Control>();
        node.Name = $"HeartbeatNode_{player.NetId}";

        var energyContainer = __instance.EnergyCounterContainer;
        if (energyContainer != null)
        {
            energyContainer.AddChild(node);
            node.Position = Vector2.Zero;
        }
        else
        {
            __instance.AddChild(node);
            node.Position = new Vector2(100, 100);
        }

        _heartbeatNodes[player.NetId] = node;

        // 创建UI时立即同步当前充能值（此时桥接能力应已挂载）
        int currentCharge = SecondaryResourceCmd.Get(player, MainFile.RellyId);
        UpdateHeartbeatNode(player, currentCharge);
    }

    public static Control? GetHeartbeatNode(ulong netId)
    {
        _heartbeatNodes.TryGetValue(netId, out var node);
        if (node != null && GodotObject.IsInstanceValid(node))
            return node;
        return null;
    }

    public static void UpdateHeartbeatNode(Player player, int amount)
    {
        var node = GetHeartbeatNode(player.NetId);
        if (node == null) return;
        var label = node.GetNodeOrNull<RichTextLabel>("HeartbeatValue");
        if (label != null) label.Text = amount.ToString();
    }

    /// <summary>
    /// 桥接能力：监听资源变化并刷新心跳UI。
    /// 完全不可见，不参与战斗逻辑。
    /// </summary>
    [RegisterPower]
    public class HeartbeatUIBridgePower : YingPower, ISecondaryResourceHookListener
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.None;
        protected override bool IsVisibleInternal => false;

        public Task AfterSecondaryResourceChanged(SecondaryResourceChangeContext context)
        {
            if (context.Definition.Id == MainFile.RellyId &&
                YingCharacterScope.IsYing(context.Player) &&
                LocalContext.IsMe(context.Player))
            {
                UpdateHeartbeatNode(context.Player, context.NewAmount);
            }
            return Task.CompletedTask;
        }
    }
}

/// <summary>
/// 在战斗开始前挂载桥接能力，确保遗物给充能前UI已能监听更新。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
public static class HeartbeatBridgeEnforcerBeforeCombat
{
    public static async void Postfix(ICombatState? combatState)
    {
        if (combatState == null) return;

        var localPlayer = LocalContext.GetMe(combatState);
        if (localPlayer == null || !YingCharacterScope.IsYing(localPlayer)) return;

        if (!localPlayer.Creature.HasPower<HeartbeatNodePatch.HeartbeatUIBridgePower>())
        {
            await PowerCmd.Apply<HeartbeatNodePatch.HeartbeatUIBridgePower>(
                new ThrowingPlayerChoiceContext(), localPlayer.Creature, 1, localPlayer.Creature, null);
        }

        int current = SecondaryResourceCmd.Get(localPlayer, MainFile.RellyId);
        HeartbeatNodePatch.UpdateHeartbeatNode(localPlayer, current);
    }
}

/// <summary>
/// 兼容旧逻辑：如果已存在能力则无需重复挂载，但保留同步。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterPlayerTurnStart))]
public static class HeartbeatBridgeEnforcer
{
    public static async void Postfix(PlayerChoiceContext choiceContext, Player player)
    {
        if (!YingCharacterScope.IsYing(player)) return;
        if (!player.Creature.HasPower<HeartbeatNodePatch.HeartbeatUIBridgePower>())
        {
            await PowerCmd.Apply<HeartbeatNodePatch.HeartbeatUIBridgePower>(
                choiceContext, player.Creature, 1, player.Creature, null);
        }

        int current = SecondaryResourceCmd.Get(player, MainFile.RellyId);
        HeartbeatNodePatch.UpdateHeartbeatNode(player, current);
    }
}