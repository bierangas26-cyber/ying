using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.HealthBars;
using STS2RitsuLib.Interop.AutoRegistration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class ShiJianZhiHuanPower : YingPower, IHealthBarForecastSource
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 🌟 删除了原本的 _stunTurnCounter 计数器，因为现在每回合都触发

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner || !props.IsPoweredAttack())
        {
            return 1m;
        }
        // 🌟 伤害加成改为 1% (0.01m)
        return 1m + (0.01m * base.Amount);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        CombatSide ownerSide = base.Owner.Side;

        if (side == ownerSide)
        {
            // 🌟 回合结束时，层数减少 1/3（向下取整，但至少减少 1 层）
            int decrease = Math.Max(1, base.Amount / 4);
            await PowerCmd.Apply<ShiJianZhiHuanPower>(choiceContext, base.Owner, -decrease, base.Owner, null);
        }
        else
        {
            // 🌟 触发条件：大于等于 (>=) 当前生命值
            if (base.Amount >= base.Owner.CurrentHp)
            {
                // 🌟 满足条件直接眩晕，不需要再数回合了！
                await CreatureCmd.Stun(base.Owner);
            }
        }
    }

    public IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
    {
        return HealthBarForecasts.Single(
            context.Creature.GetPowerAmount<ShiJianZhiHuanPower>(),
            new Color(0.1f, 0.6f, 0.9f),
            HealthBarForecastGrowthDirection.FromLeft
        );
    }
}