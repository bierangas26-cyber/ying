using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class LoseRellyNextTurnPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterEnergyReset(Player player)
    {
        if (player == base.Owner.Player)
        {
            // 1. 获取当前心脏充能数量（新资源系统）
            int currentCharge = SecondaryResourceCmd.Get(player, MainFile.RellyId);

            // 2. 防负数：最多扣除当前拥有的量
            int loseAmount = Math.Min(currentCharge, base.Amount);

            if (loseAmount > 0)
            {
                // 次要资源系统：消耗充能
                await SecondaryResourceCmd.Spend(player, MainFile.RellyId, loseAmount);
            }

            // 3. 自毁
            await PowerCmd.Remove(this);
        }
    }
}