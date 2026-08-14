using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using Yingmod.Ying.Cards;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class DanceNoteIllusionPower : IllusionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override void OnBreak()
    {
        if (Owner?.Player == null) return;

        // 尝试在安全时间点（玩家回合开始）处理添加卡牌
        // 如果在这里直接操作牌堆可能因敌人回合而失败，所以我们使用一个临时能力代为执行
        var combatState = Owner.CombatState;
        if (combatState == null) return;

        // 创建一个异步任务，确保即使应用失败也不影响破碎清理
        Task.Run(async () =>
        {
            try
            {
                // 构造一个合法的 PlayerChoiceContext（使用 HookPlayerChoiceContext）
                ulong netId = LocalContext.NetId ?? Owner.Player.NetId;
                var choiceContext = new HookPlayerChoiceContext(
                    this, netId, combatState, GameActionType.Combat);

                await PowerCmd.Apply<DanceNoteBreakPower>(
                    choiceContext, Owner, 1, Owner, null);
            }
            catch (System.Exception ex)
            {
                // 记录异常，但不中断流程
                // Logger?.Error("舞蹈音符延迟添加失败：" + ex.Message);
            }
        });
    }
}