using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
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
public class QuanHeng : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DrawAmount", 2m)
    ];

    public QuanHeng()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        // 1. 抽牌
        await CardPileCmd.Draw(choiceContext, (int)base.DynamicVars["DrawAmount"].BaseValue, base.Owner);

        // 2. 弃牌（自动或手动）
        var handPile = PileType.Hand.GetPile(base.Owner);
        CardModel? selectedCard = null;

        if (cardPlay.IsAutoPlay)
        {
            selectedCard = handPile.Cards
                .Where(c => c != this)
                .OrderBy(_ => System.Guid.NewGuid())
                .FirstOrDefault();
        }
        else
        {
            CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
            selectedCard = (await CardSelectCmd.FromHand(
                choiceContext,
                base.Owner,
                prefs,
                (CardModel c) => true,
                this
            )).FirstOrDefault();
        }

        if (selectedCard != null)
        {
            int cardCost = selectedCard.EnergyCost.GetWithModifiers(CostModifiers.All);
            if (cardCost < 0) cardCost = 0;

            await CardCmd.Discard(choiceContext, selectedCard);

            int chargeGain = cardCost * 3;
            if (chargeGain > 0)
            {
                // 次要资源系统：获得心脏充能
                await SecondaryResourceCmd.Gain(base.Owner, MainFile.RellyId, chargeGain);
            }

            SfxCmd.Play("event:/sfx/ui/card_discard", 1.0f);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["DrawAmount"].UpgradeValueBy(1m);
    }
}