using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static STS2RitsuLib.Models.HookedSingletonModel;

namespace Yingmod.Ying.Singletons;

[RegisterSingleton]
public class TimeEndSingleton : HookedSingletonModel
{
    public TimeEndSingleton() : base(HookType.Combat) { }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        var exhaustPile = PileType.Exhaust.GetPile(player);
        if (exhaustPile?.Cards == null) return;

        // 获取所有在消耗堆中的时间彼端
        var targetCards = exhaustPile.Cards
            .Where(c => c.Id.Entry == "YING_CARD_SHI_JIAN_BI_DUAN")
            .ToList();

        if (targetCards.Count == 0) return;

        foreach (var card in targetCards)
        {
            await CardCmd.AutoPlay(choiceContext, card, null);
        }
    }
}