using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class RouSuiGuangYinPower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != base.Owner) return;

        var enemies = base.Owner.CombatState.HittableEnemies;
        if (enemies.Count == 0) return;
        var target = player.RunState.Rng.CombatTargets.NextItem(enemies);
        if (target == null) return;

        // 使用能力自身的层数作为滞缓层数
        int slowAmount = (int)Amount;

        await PowerCmd.Apply<ShiJianZhiHuanPower>(choiceContext, target, slowAmount, base.Owner, null);
        await PowerCmd.Apply<RouSuiGuangYinDebuff>(choiceContext, target, 1, base.Owner, null);
    }
}