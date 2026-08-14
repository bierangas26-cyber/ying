using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public class ZhuShiWaLiExtraTurnPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    // 🌟 修正：必须改成 Counter（计数器类型），Decrement 才能生效！
    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldTakeExtraTurn(Player player)
    {
        // 🌟 修正：完全使用官方原汁原味的判定写法
        return player == base.Owner.Player;
    }

    public override async Task AfterTakingExtraTurn(Player player)
    {
        if (player == base.Owner.Player)
        {
            Flash();
            await PowerCmd.Decrement(this);  // 扣除 1 层，层数归 0 后底层会自动安全清理
        }
    }
}