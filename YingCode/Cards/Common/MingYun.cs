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

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class MingYun : YingCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(CardKeyword.Sly)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust,
        CardKeyword.Sly
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(2),
        new DynamicVar("RellyAmount", 5m),
        new DynamicVar("DrawAmount", 2m)
    ];

    public MingYun()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        // 1. 获得能量
        await PlayerCmd.GainEnergy((int)base.DynamicVars.Energy.BaseValue, base.Owner);

        // 2. 获得心脏充能（次要资源系统）
        await SecondaryResourceCmd.Gain(base.Owner, MainFile.RellyId, (int)base.DynamicVars["RellyAmount"].BaseValue);

        // 3. 抽牌
        await CardPileCmd.Draw(choiceContext, (int)base.DynamicVars["DrawAmount"].BaseValue, base.Owner);
    }

    protected override void OnUpgrade()
    {
        _ = Keywords;
        RemoveKeyword(CardKeyword.Exhaust);
    }
}