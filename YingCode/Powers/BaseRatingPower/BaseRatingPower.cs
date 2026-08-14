using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

public abstract class BaseRatingPower : YingPower
{
    protected int _turnCount = 0;
    public abstract int RatingLevel { get; }
    public virtual bool ShouldDoubleIntent() => false;
    protected virtual decimal DamageMultiplier() => 1m;
    protected virtual decimal DamageReceivedMultiplier() => 1m;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!props.IsPoweredAttack()) return 1m;
        if (dealer == Owner) return DamageMultiplier();
        if (target == Owner) return DamageReceivedMultiplier();
        return 1m;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Enemy) return;
        _turnCount++;
        await OnTurnStart();
    }

    protected virtual Task OnTurnStart() => Task.CompletedTask;

    // 在敌人回合结束时自动检查意图翻倍
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy) return;
        await TryDoubleIntent();
    }

    private async Task TryDoubleIntent()
    {
        if (!ShouldDoubleIntent()) return;

        var monster = Owner;
        if (monster.Monster?.MoveStateMachine?.StateLog == null) return;

        // 从状态日志中获取最近的一个 MoveState（即刚才执行的动作）
        var lastMove = monster.Monster.MoveStateMachine.StateLog.OfType<MoveState>().LastOrDefault();
        if (lastMove == null) return;

        // 敌人动作的目标通常是所有玩家生物
        var targets = monster.CombatState.Allies;
        if (targets == null || !targets.Any()) return;

        // 执行第二次（防重入保护）
        await lastMove.PerformMove(targets);
    }
}