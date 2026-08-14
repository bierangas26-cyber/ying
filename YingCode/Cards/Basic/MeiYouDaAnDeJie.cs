using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class MeiYouDaAnDeJie : YingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DrawCount", 4m),
        new DynamicVar("DiscardCount", 2m),
        new DynamicVar("SelfCopies", 1m)
    ];

    public MeiYouDaAnDeJie() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 抽牌
        await CardPileCmd.Draw(choiceContext, base.DynamicVars["DrawCount"].BaseValue, base.Owner);

        // 2. 选择手牌弃置
        var hand = PileType.Hand.GetPile(base.Owner);
        int toDiscard = (int)base.DynamicVars["DiscardCount"].BaseValue;
        if (hand.Cards.Count > 0 && toDiscard > 0)
        {
            int maxDiscard = Math.Min(toDiscard, hand.Cards.Count);
            var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, maxDiscard, maxDiscard);
            var selected = await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs, _ => true, this);
            if (selected.Any())
                await CardCmd.Discard(choiceContext, selected);
        }

        // 3. 将自身复制品加入弃牌堆（不再加入抽牌堆）
        int copies = (int)base.DynamicVars["SelfCopies"].BaseValue;
        for (int i = 0; i < copies; i++)
        {
            var clone = this.CreateClone();
            await CardPileCmd.Add(clone, PileType.Discard);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["DrawCount"].UpgradeValueBy(1m);
        base.DynamicVars["DiscardCount"].UpgradeValueBy(1m);
    }
}