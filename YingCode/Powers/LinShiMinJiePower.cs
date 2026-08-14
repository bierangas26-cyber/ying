using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers; // 引入敏捷
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class LinShiMinJiePower : YingPower
{
    public override PowerType Type => PowerType.Debuff;
    protected override bool IsVisibleInternal => false;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == base.Owner.Side)
        {
            // 扣除真实敏捷
            await PowerCmd.Apply<DexterityPower>(choiceContext, base.Owner, -base.Amount, base.Owner, null);
            await PowerCmd.Remove(this);
        }
    }
}