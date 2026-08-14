using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying;
using Yingmod.Ying.Keywords;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class HeiLieChePower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public int HpCost { get; set; } = 4;
    public int CopyCount { get; set; } = 2;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        // 1. 安全扣除生命（使用多目标重载，避免空引用）
        if (HpCost > 0)
        {
            var targets = new List<Creature> { Owner };
            await CreatureCmd.Damage(
                choiceContext,
                targets,
                HpCost,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                dealer: null,        // 无攻击者
                cardSource: null     // 无卡牌来源
            );
        }

        // 2. 选择一张带消耗词条的手牌
        var hand = PileType.Hand.GetPile(player);
        var exhaustCards = hand.Cards.Where(c => c.Keywords.Contains(CardKeyword.Exhaust)).ToList();
        if (exhaustCards.Any())
        {
            var prefs = new CardSelectorPrefs(
                new LocString("cards", "YING_CARD_HEI_LIE_CHE.select"), 1, 1);
            var selected = await CardSelectCmd.FromHand(choiceContext, player, prefs,
                c => c.Keywords.Contains(CardKeyword.Exhaust), this);
            var targetCard = selected.FirstOrDefault();
            if (targetCard != null)
            {
                // 3. 复制指定张数并加入轮回堆
                var rebirthPile = MainFile.RebirthPile.GetPile(player);
                if (rebirthPile != null)
                {
                    for (int i = 0; i < CopyCount; i++)
                    {
                        var clone = targetCard.CreateClone();
                        await CardPileCmd.Add(clone, rebirthPile);
                    }
                }
                // 4. 被选择的牌获得“终末”
                CardCmd.ApplyKeyword(targetCard, YingKeywords.ZhongMo);
            }
        }
    }
}