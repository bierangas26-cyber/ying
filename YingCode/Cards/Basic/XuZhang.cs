using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class XuZhang : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DrawCount", 3m),
        new DynamicVar("DiscardCount", 2m),
        new DynamicVar("RellyGain", 8m),
        new DynamicVar("SelectCount", 2m)
    ];

    public XuZhang() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 抽牌
        int drawCount = (int)base.DynamicVars["DrawCount"].BaseValue;
        if (drawCount > 0)
            await CardPileCmd.Draw(choiceContext, drawCount, base.Owner);

        // 获得心脏充能
        int rellyGain = (int)base.DynamicVars["RellyGain"].BaseValue;
        await SecondaryResourceCmd.Gain(base.Owner, MainFile.RellyId, rellyGain);

        // 玩家选择弃牌（数量由 DiscardCount 决定）
        int discardCount = (int)base.DynamicVars["DiscardCount"].BaseValue;
        var hand = PileType.Hand.GetPile(base.Owner);
        var discardable = hand.Cards.Where(c => c != this).ToList();
        if (discardable.Count > 0 && discardCount > 0)
        {
            var prefs = new CardSelectorPrefs(
                new LocString("cards", "YING_CARD_XU_ZHANG.discardSelectionScreenPrompt"),
                discardCount, discardCount);
            var selected = await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs, _ => true, this);
            if (selected.Any())
                await CardCmd.Discard(choiceContext, selected);
        }

        // 选择至多两张非永恒牌添加消耗，并各复制一张放入弃牌堆
        int selectCount = (int)base.DynamicVars["SelectCount"].BaseValue;
        var nonEternal = hand.Cards
            .Where(c => !c.Keywords.Contains(CardKeyword.Eternal) && c != this)
            .ToList();
        if (nonEternal.Count > 0 && selectCount > 0)
        {
            var prefs = new CardSelectorPrefs(
                new LocString("cards", "YING_CARD_XU_ZHANG.selectionScreenPrompt"),
                0, selectCount);   // 0 至 selectCount 张
            var chosen = await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs,
                c => !c.Keywords.Contains(CardKeyword.Eternal) && c != this, this);
            foreach (var card in chosen)
            {
                CardCmd.ApplyKeyword(card, CardKeyword.Exhaust);
                var clone = card.CreateClone();
                await CardPileCmd.Add(clone, PileType.Discard);
            }
        }
    }
}