using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class MingDingQianQiuPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // ==========================================
    // 效果1：永久增加 1 点能量上限
    // ==========================================
    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (player.Creature == base.Owner)
        {
            return amount + 1m;
        }
        return amount;
    }

    // ==========================================
    // 效果2：每当你抽一张牌时，获得心脏充能
    // ==========================================
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner.Creature == base.Owner)
        {
            // 次要资源系统：获得心脏充能
            await SecondaryResourceCmd.Gain(base.Owner.Player, MainFile.RellyId, base.Amount);
        }
    }

    // ==========================================
    // 效果3：你每打出一张牌，给敌方全体施加时间滞缓
    // ==========================================
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != base.Owner) return;

        foreach (Creature hittableEnemy in base.CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<ShiJianZhiHuanPower>(choiceContext, hittableEnemy, base.Amount, base.Owner, null);
        }
    }
}