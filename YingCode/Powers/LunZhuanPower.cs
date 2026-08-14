using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying;
using Yingmod.Ying.Cards;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class LunZhuanPower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != base.Owner) return;

        var hand = PileType.Hand.GetPile(player);
        var exhaustCards = hand.Cards.Where(c => c.Keywords.Contains(CardKeyword.Exhaust)).ToList();
        if (exhaustCards.Count == 0) return;

        int max = 1;
        var cardInPlay = player.PlayerCombatState.AllCards
            .FirstOrDefault(c => c is LunZhuan);
        if (cardInPlay is LunZhuan lunZhuan)
            max = (int)lunZhuan.DynamicVars["SelectMax"].BaseValue;

        var prefs = new CardSelectorPrefs(
            new LocString("cards", "YING_CARD_LUN_ZHUAN.select"), 0, max);
        var selected = await CardSelectCmd.FromHand(choiceContext, player, prefs,
            c => c.Keywords.Contains(CardKeyword.Exhaust), this);

        foreach (var card in selected)
        {
            var clone = card.CreateClone();
            await CardPileCmd.Add(clone, MainFile.RebirthPile);
        }
    }
}