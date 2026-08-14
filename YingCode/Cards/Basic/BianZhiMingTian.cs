using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile
using Yingmod.Ying.Character;
using Yingmod.Ying.Keywords;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class BianZhiMingTian : YingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DrawCount", 1m),
        new DynamicVar("RellyCost", 4m)    // ★ 必须保留，供补丁显示费用
    ];

    public BianZhiMingTian() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        // 设置心脏充能费用为 4
        this.SecondaryCosts().Set(MainFile.RellyId, 4);
    }

    protected override bool ShouldGlowGoldInternal =>
        SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId) >= 4;

    protected override bool IsPlayable
    {
        get
        {
            int currentCharge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
            return currentCharge >= 4 && base.IsPlayable;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 抽牌
        await CardPileCmd.Draw(choiceContext, base.DynamicVars["DrawCount"].BaseValue, base.Owner);

        // 选择一张手牌添加静滞
        var hand = PileType.Hand.GetPile(base.Owner);
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
    }

    protected override void OnUpgrade()
    {
        // 本场战斗免费：将心脏充能费用设为 Free，并持续本场战斗
        this.SecondaryCosts().Set(MainFile.RellyId, SecondaryResourceCost.Free, SecondaryResourceCostDuration.ThisCombat);
    }
}