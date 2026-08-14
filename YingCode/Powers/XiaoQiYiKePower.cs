using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class XiaoQiYiKePower : IllusionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public int RellyAmount { get; set; }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        // 次要资源系统：获得心脏充能
        await SecondaryResourceCmd.Gain(Owner.Player, MainFile.RellyId, RellyAmount);
    }
}