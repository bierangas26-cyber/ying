using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Yingmod.Ying.Patch;

[HarmonyPatch(typeof(NCombatUi), "Activate")]
public static class IllusionUIPatch
{
    private static readonly Dictionary<ulong, Control> _containers = new();

    public static void Postfix(NCombatUi __instance, CombatState state)
    {
        if (!GodotObject.IsInstanceValid(__instance)) return;

        foreach (var oldContainer in _containers.Values.ToList())
        {
            if (oldContainer != null && GodotObject.IsInstanceValid(oldContainer))
                oldContainer.QueueFree();
        }
        _containers.Clear();

        // 只为本地的 Ying 角色创建幻境容器
        var player = state.Players.OfType<Player>().FirstOrDefault(LocalContext.IsMe);
        if (player == null || player.Character.Id.Entry != "YING_CHARACTER_YING") return;

        var container = new Control
        {
            Name = "IllusionStackContainer",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            LayoutMode = 1,
            AnchorLeft = 0f,
            AnchorTop = 0f,
            AnchorRight = 0f,
            AnchorBottom = 0f,
            Position = new Vector2(120, 460)
        };
        __instance.AddChild(container);
        _containers[player.NetId] = container;
    }

    public static Control? GetContainer(ulong netId)
    {
        if (_containers.TryGetValue(netId, out var container) &&
            container != null && GodotObject.IsInstanceValid(container))
            return container;
        return null;
    }
}