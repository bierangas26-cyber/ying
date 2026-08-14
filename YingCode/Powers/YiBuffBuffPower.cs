using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class YiBuffBuffPower : YingPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.None;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == null || Owner.IsPlayer || Owner.Monster?.NextMove == null) return;

        bool hasBuffIntent = Owner.Monster.NextMove.Intents.Any(
            i => i.IntentType == IntentType.Buff);

        if (hasBuffIntent)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner, 4, player.Creature, null);
            await PowerCmd.Apply<ShiJianZhiHuanPower>(choiceContext, Owner, 20, player.Creature, null);
        }
        // 猜错不再给予惩罚

        await PowerCmd.Remove(this);
    }
}