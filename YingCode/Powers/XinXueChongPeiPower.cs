using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class XinXueChongPeiPower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == base.Owner)
        {
            int currentCharge = SecondaryResourceCmd.Get(player, MainFile.RellyId);
            int vigorToGain = (currentCharge / 6) * base.Amount;

            if (vigorToGain > 0)
            {
                Flash();
                await PowerCmd.Apply<VigorPower>(choiceContext, player.Creature, vigorToGain, player.Creature, null);
            }
        }
    }
}

public sealed class XinXueChongPeiPlusPower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == base.Owner)
        {
            int currentCharge = SecondaryResourceCmd.Get(player, MainFile.RellyId);
            int vigorToGain = (currentCharge / 4) * base.Amount;

            if (vigorToGain > 0)
            {
                Flash();
                await PowerCmd.Apply<VigorPower>(choiceContext, player.Creature, vigorToGain, player.Creature, null);
            }
        }
    }
}