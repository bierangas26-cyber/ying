using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public class YingExtraTurnPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public override bool ShouldTakeExtraTurn(Player player)
    {
        // 传入的 player（玩家）的战斗实体，是否等于这个能力的拥有者（Creature）
        return player.Creature == base.Owner;
    }

    public override async Task AfterTakingExtraTurn(Player player)
    {
        if (player.Creature == base.Owner)
        {
            Flash();

            // base.Owner 在能力类里已经是 Creature 了，直接传进去即可销毁
            await PowerCmd.Remove(this);
        }
    }
}