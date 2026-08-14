using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class FuShangJiQuPower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 每当一个能力层数发生变化时调用（包括增加、减少、施加、移除）。
    /// </summary>
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        // 只检查当次增加的是“时间滞缓”（且增加量为正）
        if (power is ShiJianZhiHuanPower && amount > 0m)
        {
            // 确保施加者是当前能力的拥有者（即玩家自己）
            if (applier == base.Owner)
            {
                // 获得7点格挡
                await CreatureCmd.GainBlock(base.Owner, 7m, ValueProp.Move, null, fast: false);
            }
        }

        await base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
    }
}