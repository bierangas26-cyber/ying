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
public sealed class FuShangJiQu : YingCard
{
    // ⚠️ 修改为 protected override，匹配基类
    protected override int CanonicalEnergyCost => IsUpgraded ? 1 : 2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("BlockAmount", 5m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<ShiJianZhiHuanPower>();
            yield return HoverTipFactory.Static(StaticHoverTip.Block);
        }
    }

    public FuShangJiQu() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.None) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FuShangJiQuPower>(
            choiceContext,
            base.Owner.Creature,
            1m,
            base.Owner.Creature,
            this
        );
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["BlockAmount"].UpgradeValueBy(2m);
    }
}