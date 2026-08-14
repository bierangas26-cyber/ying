using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Character;
using Yingmod.Ying.Keywords;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class XiangDuiJingZhi : YingCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(YingKeywords.JingZhi)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyCost", 3m)   // UI 显示用
    ];

    public XiangDuiJingZhi()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        // 设置心脏充能费用为 3
        this.SecondaryCosts().Set(MainFile.RellyId, 3);
    }

    protected override bool ShouldGlowGoldInternal =>
        SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId) >= 3;

    protected override bool IsPlayable
    {
        get
        {
            int currentCharge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
            return currentCharge >= 3 && base.IsPlayable;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 费用自动扣除

        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        // ---- 动作 1：随机使抽牌堆中一张非永恒、非消耗牌获得消耗 ----
        var drawPile = PileType.Draw.GetPile(base.Owner);
        var validDrawCards = drawPile.Cards.Where(c =>
            !c.Keywords.Contains(CardKeyword.Exhaust) &&
            !c.Keywords.Contains(CardKeyword.Eternal)
        ).ToList();

        if (validDrawCards.Count > 0)
        {
            var randomCard = validDrawCards.UnstableShuffle(
                base.Owner.RunState.Rng.CombatCardSelection).First();
            randomCard.AddKeyword(CardKeyword.Exhaust);
        }

        // ---- 动作 2：选择一张手牌获得静滞 ----
        var handPile = PileType.Hand.GetPile(base.Owner);
        var validHandCards = handPile.Cards.Where(c =>
            c != this &&
            !c.Keywords.Contains(CardKeyword.Eternal)
        ).ToList();

        if (validHandCards.Count > 0)
        {
            CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
            var selectedCards = await CardSelectCmd.FromHand(
                choiceContext,
                base.Owner,
                prefs,
                (CardModel c) => c != this && !c.Keywords.Contains(CardKeyword.Eternal),
                this
            );

            var selectedCard = selectedCards.FirstOrDefault();
            if (selectedCard != null)
            {
                selectedCard.AddKeyword(YingKeywords.JingZhi);
                SfxCmd.Play("event:/sfx/ui/card_upgrade", 1.0f);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 本场战斗免费
        this.SecondaryCosts().Set(MainFile.RellyId, SecondaryResourceCost.Free,
            SecondaryResourceCostDuration.ThisCombat);
    }
}