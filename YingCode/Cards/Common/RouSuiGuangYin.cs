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
public sealed class RouSuiGuangYin : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("SlowAmount", 14)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<ShiJianZhiHuanPower>();
            yield return HoverTipFactory.FromPower<RouSuiGuangYinDebuff>();
        }
    }

    public RouSuiGuangYin() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 从卡牌的 DynamicVar 中读取当前滞缓层数
        int slowAmount = (int)DynamicVars["SlowAmount"].BaseValue;

        // 应用能力并传入滞缓层数作为能力的堆叠数量
        await PowerCmd.Apply<RouSuiGuangYinPower>(
            choiceContext,
            base.Owner.Creature,
            slowAmount,  // 关键：传入层数
            base.Owner.Creature,
            this
        );
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["SlowAmount"].UpgradeValueBy(4);   // 14 -> 18
    }
}