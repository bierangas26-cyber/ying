using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using Yingmod.Ying;
using Yingmod.Ying.Keywords;

namespace Yingmod.Ying.Patch;

/// <summary>
/// 拦截伤害指令：时停激活时，将伤害请求存入队列而不是真正执行。
/// </summary>
[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage),
    new[] { typeof(PlayerChoiceContext), typeof(IEnumerable<Creature>), typeof(decimal), typeof(ValueProp), typeof(Creature), typeof(CardModel) })]
public static class TimeStopDamagePatch
{
    public static bool Prefix(PlayerChoiceContext choiceContext, IEnumerable<Creature> targets, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, ref System.Threading.Tasks.Task<IEnumerable<DamageResult>> __result)
    {
        if (cardSource != null)
            Log.Info($"[TimeStopDamage] Card {cardSource.Id.Entry} attacked, IsActive={TimeStopManager.IsActive}, amount={amount}, targets={targets.Count()}");

        if (!TimeStopManager.IsActive)
            return true;

        if (cardSource == null)
            return true;

        TimeStopManager.RecordDamage(choiceContext, targets, amount, props, dealer, cardSource);
        __result = System.Threading.Tasks.Task.FromResult(Enumerable.Empty<DamageResult>());
        Log.Info($"[TimeStopDamage] INTERCEPTED and recorded damage from {cardSource.Id.Entry}");
        return false;
    }
}

/// <summary>
/// 每个回合结束（正常回合或额外回合）都会触发，只要时停激活就结算。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeFlush))]
public static class TimeStopEndPatch
{
    public static async void Postfix(CombatState combatState, Player player)
    {
        if (!TimeStopManager.IsActive)
            return;

        Log.Info("[TimeStopEndPatch] BeforeFlush triggered, calling EndTimeStop.");
        await TimeStopManager.EndTimeStop();
    }
}

/// <summary>
/// 额外回合开始前，重新启动时停。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterTakingExtraTurn))]
public static class TimeStopExtraTurnStartPatch
{
    public static void Postfix(ICombatState combatState, Player player)
    {
        Log.Info($"[TimeStopExtraTurn] ExtraTurn starting for {player.NetId}, restarting time stop.");
        TimeStopManager.StartTimeStop();
    }
}

/// <summary>
/// 抽牌完成后（AfterPlayerTurnStart），将触发者所有牌堆中的静滞牌安全地补回手牌。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterPlayerTurnStart))]
public static class StasisRetrievalPatch
{
    public static async void Postfix(PlayerChoiceContext choiceContext, Player player)
    {
        if (TimeStopManager.PendingStasisRetrievalPlayerId != player.NetId)
            return;

        TimeStopManager.PendingStasisRetrievalPlayerId = null;
        Log.Info($"[StasisRetrieval] Start retrieving stasis cards for {player.NetId}");

        var handPile = PileType.Hand.GetPile(player);
        if (handPile == null) return;

        var drawPile = PileType.Draw.GetPile(player);
        var discardPile = PileType.Discard.GetPile(player);
        var exhaustPile = PileType.Exhaust.GetPile(player);
        var rebirthPile = MainFile.RebirthPile.GetPile(player);

        var allPiles = new List<CardPile>();
        if (drawPile != null) allPiles.Add(drawPile);
        if (discardPile != null) allPiles.Add(discardPile);
        if (exhaustPile != null) allPiles.Add(exhaustPile);
        if (rebirthPile != null) allPiles.Add(rebirthPile);

        var stasisCards = allPiles
            .Where(p => p != null)
            .SelectMany(p => p.Cards)
            .Where(c => c.Keywords.Contains(YingKeywords.JingZhi))
            .ToList();

        foreach (var card in stasisCards)
        {
            if (handPile.Cards.Count >= CardPile.MaxCardsInHand)
            {
                await CardPileCmd.Add(card, PileType.Draw.GetPile(player), CardPilePosition.Top);
                Log.Info($"[StasisRetrieval] {card.Id.Entry} -> draw top (hand full)");
            }
            else
            {
                await CardPileCmd.Add(card, PileType.Hand.GetPile(player));
                Log.Info($"[StasisRetrieval] {card.Id.Entry} -> hand");
            }
        }
    }
}