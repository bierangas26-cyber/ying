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
public sealed class ExpectationPower : IllusionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public int MaxHeals { get; set; } = 5;  // 默认5次
    private int _healCount = 0;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        // 1. 回血（仅当次数未满）
        if (_healCount < MaxHeals)
        {
            await CreatureCmd.Heal(Owner, 1);
            _healCount++;
        }

        // 2. 其他幻境耐久+2
        var manager = this.Manager;
        if (manager == null) return;

        foreach (var illusion in manager.Illusions)
        {
            if (illusion == this) continue;

            illusion.SetAmount(illusion.Amount + 2);
            illusion.UpdateDurabilityUI();
        }
    }
}