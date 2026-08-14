using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class AntiRealityBarrierPower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>每层提供 10% 免伤，加法叠加</summary>
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && props.IsPoweredAttack())
        {
            decimal reduction = Amount * 0.10m;
            if (reduction >= 1m) return 0m;
            return 1m - reduction;
        }
        return 1m;
    }

    private int _hitCounter = 0;

    /// <summary>每受到 10 次伤害减少 1 层</summary>
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && result.UnblockedDamage > 0)
        {
            _hitCounter++;
            if (_hitCounter >= 10)
            {
                _hitCounter = 0;
                if (Amount > 0)
                    await PowerCmd.Apply<AntiRealityBarrierPower>(choiceContext, Owner, -1, Owner, null);
            }
        }
    }

    /// <summary>
    /// 安全的层数增加，确保不会超过 5 层
    /// </summary>
    public static async Task SafeApply(PlayerChoiceContext ctx, Creature owner, int delta)
    {
        var existing = owner.GetPower<AntiRealityBarrierPower>();
        int current = existing?.Amount ?? 0;
        int toAdd = Math.Max(0, Math.Min(delta, 5 - current));   // 上限改为 5
        if (toAdd > 0)
            await PowerCmd.Apply<AntiRealityBarrierPower>(ctx, owner, toAdd, owner, null);
    }
}