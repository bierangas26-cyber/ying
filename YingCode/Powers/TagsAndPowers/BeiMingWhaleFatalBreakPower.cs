using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;
using Yingmod.Ying;
using Yingmod.Ying.Cards;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class BeiMingWhaleFatalBreakPower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        // 1. 回复 20% 最大生命值
        int healAmount = (int)(Owner.MaxHp * 0.4m);
        if (healAmount > 0)
            await CreatureCmd.Heal(Owner, healAmount);

        // 2. 最大生命值降低 10%
        int maxHpLoss = (int)(Owner.MaxHp * 0.1m);
        if (maxHpLoss > 0)
            await CreatureCmd.LoseMaxHp(choiceContext, Owner, maxHpLoss, isFromCard: false);

        // 3. 将一张悲鸣之鲸加入轮回堆
        var rebirthPile = MainFile.RebirthPile.GetPile(player);
        if (rebirthPile != null)
        {
            var combatState = player.Creature.CombatState;
            var card = combatState.CreateCard<BeiMingWhale>(player);
            await CardPileCmd.Add(card, rebirthPile);
        }

        await PowerCmd.Remove(this);
    }
}