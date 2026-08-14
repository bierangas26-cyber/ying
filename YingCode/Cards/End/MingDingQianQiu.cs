using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class MingDingQianQiu : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<MingDingQianQiuPower>(2m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromPower<ShiJianZhiHuanPower>(),
        HoverTipFactory.FromPower<MingDingQianQiuPower>()
    ];

    public MingDingQianQiu()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Buff", base.Owner.Character.CastAnimDelay);

        // 挂上命定千秋能力（该能力内部需适配新资源系统，请同时修改 MingDingQianQiuPower.cs）
        await PowerCmd.Apply<MingDingQianQiuPower>(choiceContext, base.Owner.Creature,
            base.DynamicVars["MingDingQianQiuPower"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["MingDingQianQiuPower"].UpgradeValueBy(1m);
    }
}