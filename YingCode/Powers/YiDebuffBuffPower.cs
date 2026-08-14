using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class YiDebuffBuffPower : YingPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.None;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == null || Owner.IsPlayer || Owner.Monster?.NextMove == null) return;

        bool hasDebuffIntent = Owner.Monster.NextMove.Intents.Any(
            i => i.IntentType == IntentType.Debuff || i.IntentType == IntentType.DebuffStrong);

        if (hasDebuffIntent)
        {
            await CreatureCmd.Damage(choiceContext, Owner, 15, ValueProp.Move, player.Creature, null);
            await PowerCmd.Apply<ShiJianZhiHuanPower>(choiceContext, Owner, 30, player.Creature, null);
        }
        // 猜错不再给予惩罚

        await PowerCmd.Remove(this);
    }
}