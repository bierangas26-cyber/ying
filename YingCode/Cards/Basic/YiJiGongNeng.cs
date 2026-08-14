using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class YiJiGongNeng : YingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword> { CardKeyword.Sly };

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyAmount", 7m)
    ];

    public YiJiGongNeng()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        // 次要资源系统：获得心脏充能
        await SecondaryResourceCmd.Gain(base.Owner, MainFile.RellyId,
            (int)base.DynamicVars["RellyAmount"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["RellyAmount"].UpgradeValueBy(2m);
    }
}