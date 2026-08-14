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

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class YiGangKeGang : YingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(18m, ValueProp.Move),       // 对敌伤害
        new PowerVar<StrengthPower>(1m),          // 获得力量
        new DynamicVar("SelfDamage", 5m)          // 对自己造成的伤害（固定）
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromKeyword(CardKeyword.Exhaust);
            yield return HoverTipFactory.FromPower<StrengthPower>();
        }
    }

    public YiGangKeGang() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        // 1. 攻击动画
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);

        // 2. 对目标敌人造成伤害
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx(VfxCmd.bluntPath)
            .Execute(choiceContext);

        // 3. 对自己造成可格挡的伤害
        await CreatureCmd.Damage(choiceContext, base.Owner.Creature,
            base.DynamicVars["SelfDamage"].BaseValue, ValueProp.Move, base.Owner.Creature, this);

        // 4. 获得永久力量
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature,
            base.DynamicVars.Strength.BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(6m);   // 18 → 24
    }
}