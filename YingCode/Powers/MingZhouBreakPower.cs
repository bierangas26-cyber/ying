using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;
using Yingmod.Ying.Capabilities;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class MingZhouBreakPower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        var manager = IllusionManagerCapability.GetForPlayer(player);
        if (manager == null) return;

        foreach (var illusion in manager.Illusions)
        {
            if (illusion.Amount <= 0) continue;
            illusion.SetDurability(illusion.Amount + 3);
        }

        await PowerCmd.Remove(this);
    }
}