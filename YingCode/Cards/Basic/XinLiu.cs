using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Keywords;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class XinLiu : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DrawCount", 4m),
        new DynamicVar("DiscardCount", 2m),
        new DynamicVar("DoomAmount", 3m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromPower<DoomPower>(),
        HoverTipFactory.FromKeyword(YingKeywords.JingZhi)
    ];

    public XinLiu() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 抽牌
        await CardPileCmd.Draw(choiceContext, base.DynamicVars["DrawCount"].BaseValue, base.Owner);

        // 弃牌：从手牌中随机选牌弃掉
        var hand = PileType.Hand.GetPile(base.Owner);
        var cardsToDiscard = hand.Cards.Take((int)base.DynamicVars["DiscardCount"].BaseValue).ToList();
        if (cardsToDiscard.Count > 0)
            await CardCmd.Discard(choiceContext, cardsToDiscard);

        // 选择一张手牌获得静滞
        var eligible = hand.Cards.Where(c => !c.Keywords.Contains(YingKeywords.JingZhi)).ToList();
        if (eligible.Any())
        {
            var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
            var selected = await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs,
                c => !c.Keywords.Contains(YingKeywords.JingZhi), this);
            var target = selected.FirstOrDefault();
            if (target != null)
                CardCmd.ApplyKeyword(target, YingKeywords.JingZhi);
        }

        // 自身获得灾厄
        await PowerCmd.Apply<DoomPower>(choiceContext, base.Owner.Creature,
            base.DynamicVars["DoomAmount"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["DrawCount"].UpgradeValueBy(1m);   // 3→4
    }
}