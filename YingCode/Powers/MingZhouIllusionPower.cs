using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class MingZhouIllusionPower : IllusionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public int BlockGain { get; set; }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        await CreatureCmd.GainBlock(Owner, BlockGain, ValueProp.Move, null);
    }

    // 严格按照舞蹈音符的破碎写法：在主线程异步挂载下回合+3耐久的延迟能力
    public override void OnBreak()
    {
        if (Owner?.Player == null) return;
        _ = PowerCmd.Apply<MingZhouBreakPower>(null, Owner, 1, Owner, null);
    }
}