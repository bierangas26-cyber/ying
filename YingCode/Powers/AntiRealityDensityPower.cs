using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class AntiRealityDensityPower : YingPower
{
    public override PowerType Type => PowerType.Debuff;       // 仍然为 Debuff 以符合视觉习惯
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>每层使怪物造成伤害 +1%，受到伤害 +1%</summary>
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!props.IsPoweredAttack()) return 1m;

        // 怪物造成伤害时
        if (dealer == Owner)
            return 1m + (Amount * 0.01m);

        // 怪物受到伤害时
        if (target == Owner)
            return 1m + (Amount * 0.001m);

        return 1m;
    }

    /// <summary>层数 ≥ 50 时，怪物攻击扣血后增加等于扣血量的密度</summary>
    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer == Owner && result.UnblockedDamage > 0 && Amount >= 50)
        {
            await PowerCmd.Apply<AntiRealityDensityPower>(choiceContext, Owner, result.UnblockedDamage, Owner, null);
        }
    }
}