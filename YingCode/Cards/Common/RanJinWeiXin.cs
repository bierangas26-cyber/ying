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
public class RanJinWeiXin : YingCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyAmount", 8m)
    ];

    public RanJinWeiXin()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);

        CardModel selectedCard = (await CardSelectCmd.FromHand(
            choiceContext,
            base.Owner,
            prefs,
            (CardModel c) => c != this,
            this
        )).FirstOrDefault();

        if (selectedCard != null)
        {
            await CardCmd.Exhaust(choiceContext, selectedCard);
            // 次要资源系统：获得心脏充能
            await SecondaryResourceCmd.Gain(base.Owner, MainFile.RellyId,
                (int)base.DynamicVars["RellyAmount"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["RellyAmount"].UpgradeValueBy(4m);
    }
}