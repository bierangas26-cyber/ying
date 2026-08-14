using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying;
using Yingmod.Ying.Keywords;

namespace Yingmod.Ying.Patches;

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class ZhongMoGlobalPatch
{
    private static readonly HashSet<ModelId> _processingIds = new();

    public static async void Postfix(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 只对 Ying 角色生效
        if (cardPlay.Card.Owner?.Character.Id.Entry != "YING_CHARACTER_YING") return;
        if (!cardPlay.Card.Keywords.Contains(YingKeywords.ZhongMo)) return;

        var cardId = cardPlay.Card.Id;

        lock (_processingIds)
        {
            if (_processingIds.Contains(cardId)) return;
            _processingIds.Add(cardId);
        }

        try
        {
            var rebirthPile = MainFile.RebirthPile.GetPile(cardPlay.Card.Owner);
            if (rebirthPile?.Cards == null) return;

            var matchingCards = rebirthPile.Cards
                .Where(c => c.Id == cardId && c != cardPlay.Card)
                .ToList();

            foreach (var copy in matchingCards)
            {
                var target = cardPlay.Target;
                if (target != null && target.IsDead)
                    target = null;

                await CardCmd.AutoPlay(choiceContext, copy, target);
            }
        }
        finally
        {
            lock (_processingIds)
            {
                _processingIds.Remove(cardId);
            }
        }
    }
}