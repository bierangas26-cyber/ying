using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class XiChen : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyCost", 5m),      // 消耗 5 点心脏充能（UI 显示用）
        new DynamicVar("DrawAmount", 3m),
        new DynamicVar("DiscardAmount", 2m)
    ];

    public XiChen()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        // 设置心脏充能费用为 5
        this.SecondaryCosts().Set(MainFile.RellyId, 5);
    }

    protected override bool ShouldGlowGoldInternal =>
        SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId) >= 5;

    protected override bool IsPlayable
    {
        get
        {
            int currentCharge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
            return currentCharge >= 5 && base.IsPlayable;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 心脏充能费用由次要资源系统自动扣除

        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        // 1. 抽牌
        await CardPileCmd.Draw(choiceContext, (int)base.DynamicVars["DrawAmount"].BaseValue, base.Owner);

        var handPile = PileType.Hand.GetPile(base.Owner);

        // 2. 弃牌
        int discardCount = (int)base.DynamicVars["DiscardAmount"].BaseValue;
        if (handPile.Cards.Any(c => c != this))
        {
            CardSelectorPrefs discardPrefs = new CardSelectorPrefs(base.SelectionScreenPrompt, discardCount);
            var selectedToDiscard = await CardSelectCmd.FromHand(
                choiceContext, base.Owner, discardPrefs,
                (CardModel c) => c != this, this);

            foreach (var c in selectedToDiscard)
                await CardCmd.Discard(choiceContext, c);

            if (selectedToDiscard.Any())
                SfxCmd.Play("event:/sfx/ui/card_discard", 1.0f);
        }

        // 3. 选择一张手牌本回合免费打出
        if (handPile.Cards.Any(c => c != this))
        {
            LocString freePrompt = new LocString("cards", base.Id.Entry + ".freeScreenPrompt");
            CardSelectorPrefs freePrefs = new CardSelectorPrefs(freePrompt, 1);

            var selectedToFree = await CardSelectCmd.FromHand(
                choiceContext, base.Owner, freePrefs,
                (CardModel c) => c != this, this);

            CardModel cardToFree = selectedToFree.FirstOrDefault();
            if (cardToFree != null)
            {
                // 使用官方费用修饰器，本回合免费
                cardToFree.EnergyCost.SetThisTurnOrUntilPlayed(0);
                SfxCmd.Play("event:/sfx/ui/card_upgrade", 1.0f);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["DrawAmount"].UpgradeValueBy(1m);
    }
}