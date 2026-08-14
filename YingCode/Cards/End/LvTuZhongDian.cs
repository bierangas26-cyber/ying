using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class LvTuZhongDian : YingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new MaxHpVar(1m),
        new DamageVar(23m, ValueProp.Move),
        new DynamicVar("Multiplier", 2m),
        new PowerVar<StrengthPower>(-1m), // 减少敌方力量
        new PowerVar<WeakPower>(1m)        // 施加虚弱层数基数
    ];

    public LvTuZhongDian() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 扣除生命上限
        await CreatureCmd.LoseMaxHp(choiceContext, base.Owner.Creature, base.DynamicVars.MaxHp.BaseValue, isFromCard: true);

        // 统计手牌中带有消耗词条的牌的数量（排除自身）
        var hand = PileType.Hand.GetPile(base.Owner);
        int x = hand?.Cards.Count(c => c != this && c.Keywords.Contains(CardKeyword.Exhaust)) ?? 0;
        if (x == 0) return;

        int damageTimes = (int)base.DynamicVars["Multiplier"].BaseValue * x;

        // 对所有敌人造成伤害 damageTimes 次
        foreach (var enemy in base.CombatState.HittableEnemies)
        {
            for (int i = 0; i < damageTimes; i++)
                await CreatureCmd.Damage(choiceContext, enemy, base.DynamicVars.Damage.BaseValue, ValueProp.Move, base.Owner.Creature, this);
        }

        // 全体敌人失去 x 层力量（施加负值）
        foreach (var enemy in base.CombatState.HittableEnemies)
            await PowerCmd.Apply<StrengthPower>(choiceContext, enemy, -x, base.Owner.Creature, this);

        // 全体敌人施加 x 层虚弱
        foreach (var enemy in base.CombatState.HittableEnemies)
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, x * (int)base.DynamicVars.Weak.BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal); // 移除虚无
    }
}