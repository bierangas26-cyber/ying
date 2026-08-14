using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers; // 引入力量
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class LinShiLiLiangPower : YingPower
{
    public override PowerType Type => PowerType.Debuff; // 红色负面图标
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override bool IsVisibleInternal => false;

    // 监听回合结束的钩子
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // 只有在自己的回合结束时才触发
        if (side == base.Owner.Side)
        {
            // 1. 扣除对应层数的真实力量
            await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner, -base.Amount, base.Owner, null);

            // 2. 使这个临时能力彻底销毁
            await PowerCmd.Remove(this);
        }
    }
}