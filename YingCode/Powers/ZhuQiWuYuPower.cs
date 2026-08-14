using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;
using Yingmod.Ying.Cards;   // 之后替换为真实幻境牌

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class ZhuQiWuYuPower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        // 联机安全的战斗卡牌生成随机数
        var rng = player.RunState.Rng.CombatCardGeneration;

        // 候选幻境牌（占位）
        CardModel[] candidates = new CardModel[] {
            ModelDb.Card<ZhuQiWuYu>().ToMutable(),
            ModelDb.Card<ZhuQiWuYu>().ToMutable(),
            ModelDb.Card<ZhuQiWuYu>().ToMutable()
        };

        int index = rng.NextInt(0, candidates.Length);
        var card = candidates[index].ToMutable();

        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
    }
}