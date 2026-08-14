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
using Yingmod.Ying.Cards;
using Yingmod.Ying.Keywords;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class ZhongZhangPower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public int DrawCount { get; set; } = 2;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != base.Owner) return;

        // 1. 额外抽牌
        if (DrawCount > 0)
            await CardPileCmd.Draw(choiceContext, DrawCount, player);

        // 2. 选择抽牌堆中 0~1 张带有消耗词条的牌，附加终末
        var drawPile = PileType.Draw.GetPile(player);
        var exhaustCards = drawPile.Cards.Where(c => c.Keywords.Contains(CardKeyword.Exhaust)).ToList();
        if (exhaustCards.Any())
        {
            var prefs = new CardSelectorPrefs(
                new LocString("cards", "YING_CARD_ZHONG_ZHANG.select"), 0, 1);
            var selected = await CardSelectCmd.FromCombatPile(choiceContext, drawPile, player, prefs,
                c => c.Keywords.Contains(CardKeyword.Exhaust));
            foreach (var card in selected)
            {
                CardCmd.ApplyKeyword(card, YingKeywords.ZhongMo);
            }
        }
    }
}