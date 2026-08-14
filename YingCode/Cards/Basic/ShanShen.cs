using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class ShanShen : YingCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromKeyword(CardKeyword.Sly),       // 提示 奇巧
        HoverTipFactory.FromPower<DexterityPower>()         // 提示 敏捷
    ];

    // 天生自带 奇巧(Sly) 词条
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Sly
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(13m, ValueProp.Move),         // 基础获得 13 点格挡
        new PowerVar<DexterityPower>(2m)           // 基础获得 2 点敏捷
    ];

    public ShanShen()
        : base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 播放防御动画
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Defend", base.Owner.Character.CastAnimDelay);

        // 1. 获得 13 点格挡（致敬官方 Compact.cs 的标准传参写法）
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);

        // 提取动态敏捷数值
        decimal dexAmount = base.DynamicVars["DexterityPower"].BaseValue;

        // 2. 获得 2 点真实敏捷
        await PowerCmd.Apply<DexterityPower>(choiceContext, base.Owner.Creature, dexAmount, base.Owner.Creature, this);

        // 3. 获得 2 层你写的“临时敏捷”隐藏状态，回合末自动把敏捷扣回去！
        await PowerCmd.Apply<LinShiMinJiePower>(choiceContext, base.Owner.Creature, dexAmount, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级效果：此处我设定为格挡 13 -> 17，敏捷提升保持 2 点。你可以按需调整。
        base.DynamicVars.Block.UpgradeValueBy(4m);
        // 如果想升级增加敏捷，可以加一句：base.DynamicVars["DexterityPower"].UpgradeValueBy(1m);
    }
}