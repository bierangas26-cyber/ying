using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
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
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Character;
using Yingmod.Ying.Keywords;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class ErJiGongNeng : YingCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromKeyword(YingKeywords.JingZhi)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword> { YingKeywords.JingZhi };

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyAmount", 10m),
        new DynamicVar("BonusAmount", 6m)
    ];

    public ErJiGongNeng()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        int totalGain = (int)base.DynamicVars["RellyAmount"].BaseValue;

        int cardsPlayed = CombatManager.Instance.History.CardPlaysFinished.Count(
            (CardPlayFinishedEntry e) => e.HappenedThisTurn(base.CombatState) && e.CardPlay.Card.Owner == base.Owner
        );

        if (cardsPlayed == 0)
        {
            totalGain += (int)base.DynamicVars["BonusAmount"].BaseValue;
        }

        await SecondaryResourceCmd.Gain(base.Owner, MainFile.RellyId, totalGain);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["RellyAmount"].UpgradeValueBy(2m);
    }
}