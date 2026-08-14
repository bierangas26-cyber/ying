using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Yingmod.Ying.Keywords;

namespace Yingmod.Ying.Patch;

/// <summary>
/// 静滞能力牌完全免费：跳过资源消耗并清零充能费用
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.SpendResources))]
public static class JingZhiFreePowerPatch
{
    public static bool Prefix(CardModel __instance, ref System.Threading.Tasks.Task<(int, int)> __result)
    {
        if (!__instance.Keywords.Contains(YingKeywords.JingZhi))
            return true;

        if (__instance.Type != CardType.Power)
            return true;

        // 清零所有心脏充能消耗变量，确保 OnPlay 阶段也不扣充能
        foreach (var dv in __instance.DynamicVars.Values)
        {
            if (dv.Name is "RellyPower" or "RellyCost" or "CostAmount")
                dv.BaseValue = 0;
        }

        // 跳过原方法，不消耗能量和星星
        __result = System.Threading.Tasks.Task.FromResult((0, 0));
        return false;
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class JingZhiKeywordPatcher
{
    private static readonly Dictionary<string, int> _playCounts = new();

    public static int GetPlayCount(string key) => _playCounts.TryGetValue(key, out int c) ? c : 0;

    public static void Postfix(ICombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (!card.Keywords.Contains(YingKeywords.JingZhi) || card.Owner == null)
            return;

        string key = $"{card.Owner.NetId}_{card.GetHashCode()}";
        int count = _playCounts.TryGetValue(key, out var c) ? c : 0;
        count++;
        _playCounts[key] = count;

        // 能力牌免费打出后消失，不参与后续减费
        if (card.Type == CardType.Power)
            return;

        // 每次打出能量费用减1，最低0
        int currentCanonical = card.EnergyCost.Canonical;
        int newBaseCost = Math.Max(0, currentCanonical - 1);
        if (newBaseCost < currentCanonical)
            card.EnergyCost.SetCustomBaseCost(newBaseCost);

        // 检查是否有心脏充能消耗
        foreach (var dv in card.DynamicVars.Values)
        {
            if (dv.Name is "RellyPower" or "RellyCost" or "CostAmount")
            {
                if (dv.BaseValue > 0)
                {
                    // 每打出1次，充能消耗减半，向下取整
                    dv.BaseValue = Math.Floor(dv.BaseValue / 2);
                }
                break;
            }
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
    [HarmonyPostfix]
    public static void BeforeCombatStart()
    {
        _playCounts.Clear();
    }

    [HarmonyPatch(typeof(CardModel), "get_HoverTips")]
    public static class JingZhiHoverTipPatch
    {
        public static void Postfix(CardModel __instance, ref IEnumerable<IHoverTip> __result)
        {
            if (!__instance.Keywords.Contains(YingKeywords.JingZhi) || __instance.Owner == null)
                return;

            string key = $"{__instance.Owner.NetId}_{__instance.GetHashCode()}";
            int count = JingZhiKeywordPatcher.GetPlayCount(key);

            var title = new LocString("static_hover_tips", "YING_STATIC_HOVER_JINGZHI_COUNT.title");
            var tip = new HoverTip(title, $"当前打出：{count}次");

            __result = __result.Concat(new IHoverTip[] { tip });
        }
    }
}