using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class RatingD : BaseRatingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override int RatingLevel => 1;
    protected override decimal DamageMultiplier() => 1.20m;   // 20% 增伤

    protected override async Task OnTurnStart()
    {
        await PowerCmd.Apply<AntiRealityDensityPower>(new ThrowingPlayerChoiceContext(), Owner, 1, Owner, null);
    }
}