using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class WeiDuDieJia : YingCard
{
    public WeiDuDieJia() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var drawPile = PileType.Draw.GetPile(base.Owner);
        var exhaustCards = drawPile.Cards.Where(c => c.Keywords.Contains(CardKeyword.Exhaust)).ToList();
        if (exhaustCards.Count == 0) return;

        int toSelect = System.Math.Min(2, exhaustCards.Count);
        var rng = base.Owner.RunState.Rng.CombatCardSelection;
        int replayAmount = base.IsUpgraded ? 3 : 2;   // 基础2层，升级后3层
        var chosen = new List<CardModel>();
        for (int i = 0; i < toSelect; i++)
        {
            var card = rng.NextItem(exhaustCards);
            if (card != null)
            {
                card.BaseReplayCount += replayAmount;
                chosen.Add(card);
                exhaustCards.Remove(card);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.SetThisCombat(1);   // 费用变为1
    }
}