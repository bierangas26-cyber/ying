using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Capabilities;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class JiBanIllusionPower : IllusionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        var manager = this.Manager;
        if (manager == null) return;
        int count = manager.Illusions.Count;

        if (count > 0)
        {
            // 1. 次要资源系统：获得心脏充能
            await SecondaryResourceCmd.Gain(Owner.Player, MainFile.RellyId, count);

            // 2. 增加真实力量
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, count, Owner, null);

            // 3. 挂载临时力量标记
            await PowerCmd.Apply<LinShiLiLiangPower>(choiceContext, Owner, count, Owner, null);
        }
    }

    public override void OnBreak()
    {
        if (Owner?.Player == null) return;
        _ = PowerCmd.Apply<JiBanBreakPower>(null, Owner, 1, Owner, null);
    }
}