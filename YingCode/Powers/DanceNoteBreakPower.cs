using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;
using Yingmod.Ying;          // MainFile.RebirthPile
using Yingmod.Ying.Cards;     // DanceNote 卡牌类

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class DanceNoteBreakPower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        var rebirthPile = MainFile.RebirthPile.GetPile(player);
        if (rebirthPile != null)
        {
            // 显式指定类型参数
            var combatState = player.Creature.CombatState;
            var card = combatState.CreateCard<DanceNote>(player);
            await CardPileCmd.Add(card, rebirthPile);
        }

        await PowerCmd.Remove(this);
    }
}