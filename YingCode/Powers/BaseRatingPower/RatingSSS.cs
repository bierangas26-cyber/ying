using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class RatingSSS : BaseRatingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override int RatingLevel => 7;
    protected override decimal DamageMultiplier() => 1.50m;

    public override bool ShouldDoubleIntent() => _turnCount % 2 == 0;

    protected override async Task OnTurnStart()
    {
        await PowerCmd.Apply<AntiRealityDensityPower>(new ThrowingPlayerChoiceContext(), Owner, 4, Owner, null);
        await AntiRealityBarrierPower.SafeApply(new ThrowingPlayerChoiceContext(), Owner, 1);   // 每回合 +1
        await PassivationPower.SafeApply(new ThrowingPlayerChoiceContext(), Owner, 1);
        if (_turnCount % 3 == 0)
            await CreatureCmd.Heal(Owner, (int)(Owner.MaxHp * 0.30));
    }
}