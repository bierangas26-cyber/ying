using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class PassivationPower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public const int MaxStacks = 50;

    // 每层提供 1.3% 免伤，加法叠加
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && props.IsPoweredAttack())
        {
            decimal reduction = Amount * 0.013m;
            if (reduction >= 1m) return 0m;
            return 1m - reduction;
        }
        return 1m;
    }

    // 每受到实际伤害 +1 层，上限 50
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && result.UnblockedDamage > 0 && Amount < MaxStacks)
        {
            await PowerCmd.Apply<PassivationPower>(choiceContext, Owner, 1, Owner, null);
        }
    }

    // 回合结束时减少当前层数的 50%（向下取整）
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy && participants.Contains(Owner))
        {
            int decrease = Amount / 2;  // 向下取整
            if (decrease > 0)
                await PowerCmd.Apply<PassivationPower>(choiceContext, Owner, -decrease, Owner, null);
        }
    }

    /// <summary>
    /// 安全增加层数，确保不超过 50 层上限。
    /// </summary>
    public static async Task SafeApply(PlayerChoiceContext ctx, Creature owner, int delta)
    {
        var existing = owner.GetPower<PassivationPower>();
        int current = existing?.Amount ?? 0;
        int toAdd = Math.Max(0, Math.Min(delta, MaxStacks - current));
        if (toAdd > 0)
            await PowerCmd.Apply<PassivationPower>(ctx, owner, toAdd, owner, null);
    }
}