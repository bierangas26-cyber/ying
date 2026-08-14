using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class RatingB : BaseRatingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override int RatingLevel => 3;
    protected override decimal DamageMultiplier() => 1.35m;          // 35% 增伤
    protected override decimal DamageReceivedMultiplier() => 1.50m; // 50% 易伤

    public override bool ShouldDoubleIntent() => _turnCount % 3 == 0;

    protected override async Task OnTurnStart()
    {
        await PowerCmd.Apply<AntiRealityDensityPower>(new ThrowingPlayerChoiceContext(), Owner, 2, Owner, null);
    }
}