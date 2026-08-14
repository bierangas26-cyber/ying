using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class ShiJianZhongYan : YingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new MaxHpVar(1m),   // 扣除10点最大生命
        new DamageVar(0m, ValueProp.Move)  // 伤害基于滞缓层数，不可格挡
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromKeyword(CardKeyword.Ethereal);
            yield return HoverTipFactory.FromPower<ShiJianZhiHuanPower>();
        }
    }

    public ShiJianZhongYan() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        // 1. 扣除最大生命
        await CreatureCmd.LoseMaxHp(choiceContext, base.Owner.Creature, base.DynamicVars.MaxHp.BaseValue, isFromCard: true);

        // 2. 获取目标当前滞缓层数作为伤害
        int slowAmount = cardPlay.Target.GetPower<ShiJianZhiHuanPower>()?.Amount ?? 0;
        if (slowAmount > 0)
        {
            await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);
            await CreatureCmd.Damage(choiceContext, cardPlay.Target, slowAmount, ValueProp.Move, base.Owner.Creature, this);
        }
    }

    // 无需升级
}