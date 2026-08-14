using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models; // 🌟 核心：引入 ModelDb 所在的命名空间
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class ShanBian : YingCard
{
    // 🌟 完美参考 SakuraRed02.cs：把《命运》的卡面直接贴在《嬗变》旁边作为预览！
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromPower<DoomPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromPower<ShanBianPower>(),
        HoverTipFactory.FromCard(ModelDb.Card<MingYun>()) // 🎯 核心修复：直接呼出《命运》的缩略卡面提示
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<ShanBianPower>(3m)
    ];

    public ShanBian()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        // 施加能力
        await PowerCmd.Apply<ShanBianPower>(choiceContext, base.Owner.Creature, base.DynamicVars["ShanBianPower"].BaseValue, base.Owner.Creature, this);

        var power = base.Owner.Creature.GetPower<ShanBianPower>();
        if (power != null && this.IsUpgraded)
        {
            power.ProducesUpgradedMingYun = true;
        }
    }
}