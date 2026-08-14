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
public class JinJiGongNeng : YingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword> { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1),               // 整数值 1 点能量
        new DynamicVar("RellyAmount", 4m),
        new DynamicVar("DrawAmount", 1m)
    ];

    public JinJiGongNeng()
        : base(0, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        // 从变量读取能量值并转为 int
        await PlayerCmd.GainEnergy((int)base.DynamicVars.Energy.BaseValue, base.Owner);

        // 获得心脏充能（次要资源系统）
        await SecondaryResourceCmd.Gain(base.Owner, MainFile.RellyId,
            (int)base.DynamicVars["RellyAmount"].BaseValue);

        await CardPileCmd.Draw(choiceContext, (int)base.DynamicVars["DrawAmount"].BaseValue, base.Owner);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}