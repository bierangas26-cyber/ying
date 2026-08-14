using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers; // VulnerablePower, WeakPower
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class RuoDianJiPo : YingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(16m, ValueProp.Move),          // 基础伤害 16
        new DynamicVar("LinShiLiLiang", 3m),         // 临时力量 3（用普通变量代替PowerVar）
        new PowerVar<VulnerablePower>(3m),           // 易伤 3
        new PowerVar<WeakPower>(3m),                 // 虚弱 3
        new DynamicVar("SlowAmount", 12m)            // 时间滞缓 12
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromKeyword(CardKeyword.Exhaust);
            yield return HoverTipFactory.FromPower<LinShiLiLiangPower>();
            yield return HoverTipFactory.FromPower<VulnerablePower>();
            yield return HoverTipFactory.FromPower<WeakPower>();
            yield return HoverTipFactory.FromPower<ShiJianZhiHuanPower>();
        }
    }

    public RuoDianJiPo() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        // 1. 播放攻击动画
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);

        // 2. 造成伤害
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx(VfxCmd.bluntPath)
            .Execute(choiceContext);

        // 3. 先增加真实力量
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature,
            base.DynamicVars["LinShiLiLiang"].BaseValue, base.Owner.Creature, this);

        // 4. 再施加临时力量标记（回合结束时自动扣回等量力量）
        await PowerCmd.Apply<LinShiLiLiangPower>(choiceContext, base.Owner.Creature,
            base.DynamicVars["LinShiLiLiang"].BaseValue, base.Owner.Creature, this);

        // 5. 施加易伤、虚弱、时间滞缓
        await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target,
            base.DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target,
            base.DynamicVars.Weak.BaseValue, base.Owner.Creature, this);
        await PowerCmd.Apply<ShiJianZhiHuanPower>(choiceContext, cardPlay.Target,
            base.DynamicVars["SlowAmount"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级：伤害 +3（16→19），时间滞缓 +8（12→20）
        base.DynamicVars.Damage.UpgradeValueBy(3m);
        base.DynamicVars["SlowAmount"].UpgradeValueBy(8m);
    }
}