using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class MingXiaoGuMeng : YingCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
        HoverTipFactory.FromPower<LoseRellyNextTurnPower>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Ethereal
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyAmount", 46m)
    ];

    public MingXiaoGuMeng()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        int amount = (int)base.DynamicVars["RellyAmount"].BaseValue;

        // 1. 本回合获得充能（新系统）
        await SecondaryResourceCmd.Gain(base.Owner, MainFile.RellyId, amount);

        // 2. 下回合扣除等量充能（该能力内部可能仍需适配，但功能保留）
        await PowerCmd.Apply<LoseRellyNextTurnPower>(choiceContext, base.Owner.Creature, amount, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        _ = Keywords;
        RemoveKeyword(CardKeyword.Ethereal);
    }
}