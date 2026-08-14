using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Linq;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class YiDefendBuffPower : YingPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.None;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == null || Owner.IsPlayer || Owner.Monster?.NextMove == null) return;

        bool hasDefendIntent = Owner.Monster.NextMove.Intents.Any(
            i => i.IntentType == IntentType.Defend);

        if (hasDefendIntent)
        {
            await CreatureCmd.Stun(Owner);
        }
        // 猜错不再给予惩罚

        await PowerCmd.Remove(this);
    }
}