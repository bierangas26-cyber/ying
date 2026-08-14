using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class RatingSS : BaseRatingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override int RatingLevel => 6;
    protected override decimal DamageMultiplier() => 1.40m;          // 40% 增伤
    protected override decimal DamageReceivedMultiplier() => 0.70m; // 30% 免伤

    public override bool ShouldDoubleIntent() => _turnCount % 3 == 0;

    protected override async Task OnTurnStart()
    {
        await PowerCmd.Apply<AntiRealityDensityPower>(new ThrowingPlayerChoiceContext(), Owner, 3, Owner, null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, 1, Owner, null);
        if (_turnCount % 3 == 0)
            await CreatureCmd.Heal(Owner, (int)(Owner.MaxHp * 0.20));
    }
}