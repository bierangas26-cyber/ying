using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class ShiJianLvRen : YingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword> { CardKeyword.Sly };

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyAmount", 11m)
    ];

    public ShiJianLvRen()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        // 次要资源系统：获得心脏充能
        await SecondaryResourceCmd.Gain(base.Owner, MainFile.RellyId,
            (int)base.DynamicVars["RellyAmount"].BaseValue);
    }

    public override async Task BeforeSideTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && this.Pile != null && this.Pile.Type == PileType.Discard)
        {
            SfxCmd.Play("event:/sfx/ui/card_upgrade", 1.0f);
            decimal growth = base.IsUpgraded ? 4m : 3m;
            base.DynamicVars["RellyAmount"].BaseValue += growth;
            await CardPileCmd.Add(this, PileType.Draw, CardPilePosition.Top);
        }
    }

    protected override void OnUpgrade()
    {
    }
}