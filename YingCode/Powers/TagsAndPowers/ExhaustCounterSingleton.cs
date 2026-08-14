using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using System.Threading.Tasks;
using static STS2RitsuLib.Models.HookedSingletonModel;

namespace Yingmod.Ying.Singletons;

[RegisterSingleton]
public class ExhaustCounterSingleton : HookedSingletonModel
{
    public static int PlayedExhaustCountThisTurn { get; private set; }

    public ExhaustCounterSingleton() : base(HookType.Combat) { }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Keywords.Contains(CardKeyword.Exhaust))
            PlayedExhaustCountThisTurn++;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        PlayedExhaustCountThisTurn = 0;
    }
}