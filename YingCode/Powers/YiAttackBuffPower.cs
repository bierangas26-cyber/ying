using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Linq;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class YiAttackBuffPower : YingPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.None;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == null || Owner.IsPlayer || Owner.Monster?.NextMove == null) return;

        bool hasAttackIntent = Owner.Monster.NextMove.Intents.Any(
            i => i.IntentType == IntentType.Attack);

        if (hasAttackIntent)
        {
            var playerCreature = player.Creature;
            await CreatureCmd.GainBlock(playerCreature, 10, ValueProp.Move, null);
            await PowerCmd.Apply<WeakPower>(choiceContext, Owner, 2, playerCreature, null);
            await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner, 2, playerCreature, null);
        }
        // 猜错不再给予惩罚

        await PowerCmd.Remove(this);
    }
}