using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class RatingS : BaseRatingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override int RatingLevel => 5;
    protected override decimal DamageMultiplier() => 1.40m;   // 40% 增伤

    protected override async Task OnTurnStart()
    {
        await PowerCmd.Apply<AntiRealityDensityPower>(new ThrowingPlayerChoiceContext(), Owner, 3, Owner, null);
        await CreatureCmd.Heal(Owner, (int)(Owner.MaxHp * 0.10));
    }
}