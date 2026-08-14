using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using System.Linq;
using System.Threading.Tasks;
using static STS2RitsuLib.Models.HookedSingletonModel;

namespace Yingmod.Ying.Singletons;

[RegisterSingleton]
public class LunHuiReturnSingleton : HookedSingletonModel
{
    public LunHuiReturnSingleton() : base(HookType.Combat) { }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        var lunHuiCard = player.PlayerCombatState.AllCards
            .FirstOrDefault(c => c.Id.Entry == "YING_CARD_LUN_HUI");
        if (lunHuiCard != null)
        {
            var handPile = PileType.Hand.GetPile(player);
            if (!handPile.Cards.Contains(lunHuiCard))
            {
                await CardPileCmd.Add(lunHuiCard, PileType.Hand);
            }
        }
    }
}