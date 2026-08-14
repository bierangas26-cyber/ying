using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using Yingmod.Ying.Capabilities;
using Yingmod.Ying.Helpers;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Patch;

[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyHpLost))]
public static class IllusionDamagePatch
{
    private static readonly HashSet<ulong> _processing = new();

    public static void Prefix(Creature target, ref decimal amount)
    {
        if (target?.Player == null) return;

        var player = target.Player;
        if (!YingCharacterScope.IsYing(player)) return;

        ulong netId = player.NetId;
        if (_processing.Contains(netId)) return;

        var manager = IllusionManagerCapability.GetForPlayer(player);
        if (manager == null) return;

        var illusions = manager.Illusions;
        if (illusions.Count == 0) return;

        // 致命伤害：寻找任意悲鸣之鲸 → 完全免疫
        if (target.CurrentHp - amount <= 0)
        {
            var whale = illusions.FirstOrDefault(p => p is BeiMingWhalePower && p.Amount > 0) as BeiMingWhalePower;
            if (whale != null)
            {
                _processing.Add(netId);
                try { whale.OnFatalBreak(); amount = 0; }
                finally { _processing.Remove(netId); }
                return;
            }
        }

        _processing.Add(netId);
        try
        {
            var first = illusions[0];
            int dur = first.Amount;
            if (dur <= 0)
            {
                // 清理空耐久幻境（先安全移除 UI 再移除能力）
                SafeRemoveIllusionNode(manager, first);
                manager.RemoveIllusionPower(first);
                _ = MegaCrit.Sts2.Core.Commands.PowerCmd.Remove(first);
                return;
            }

            if (first is DanceNoteIllusionPower)
            {
                int absorb = (int)Math.Min(amount, dur);
                first.ReduceDurability(absorb);
                amount -= absorb;

                if (first.Amount <= 0)
                    BreakIllusionSafely(manager, first);
            }
            else
            {
                int damageToDurability = (int)Math.Min(amount, dur);
                first.ReduceDurability(damageToDurability);

                if (first.Amount <= 0)
                    BreakIllusionSafely(manager, first);
            }
        }
        finally { _processing.Remove(netId); }
    }

    // 带有效性检查的幻境破碎
    private static void BreakIllusionSafely(IllusionManagerCapability manager, IllusionPower power)
    {
        power.OnBreak();                     // 先触发自定义效果
        SafeRemoveIllusionNode(manager, power);
        manager.RemoveIllusionPower(power);
        _ = MegaCrit.Sts2.Core.Commands.PowerCmd.Remove(power);
    }

    // 安全移除幻境 UI 节点
    private static void SafeRemoveIllusionNode(IllusionManagerCapability manager, IllusionPower power)
    {
        var node = power.IllusionNode;
        if (node != null && GodotObject.IsInstanceValid(node))
        {
            manager.RemoveIllusionUI(node);
        }
        power.IllusionNode = null;
    }
}