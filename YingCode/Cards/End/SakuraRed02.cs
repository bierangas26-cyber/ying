using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class SakuraRed02 : YingCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromCard(ModelDb.Card<FenSuiXianShi>())
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword> { CardKeyword.Exhaust };

    public SakuraRed02()
        : base(3, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        // 1. 读取当前心脏充能总量（新资源系统）
        int totalRelly = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);

        // 2. 清空手牌（除了自己）
        var handPile = PileType.Hand.GetPile(base.Owner);
        while (handPile.Cards.Any(c => c != this))
        {
            CardModel nextCard = handPile.Cards.FirstOrDefault(c => c != this);
            if (nextCard != null)
            {
                await CardCmd.Discard(choiceContext, nextCard);
            }
        }

        // 3. 每 10 点充能生成一张“粉碎现实”
        int cardsToAdd = totalRelly / 10;
        if (cardsToAdd > 0)
        {
            for (int i = 0; i < cardsToAdd; i++)
            {
                CardModel fenSuiCard = base.CombatState.CreateCard<FenSuiXianShi>(base.Owner);
                await CardPileCmd.Add(fenSuiCard, PileType.Hand);
            }
        }

        // 4. 消耗全部心脏充能
        if (totalRelly > 0)
        {
            await SecondaryResourceCmd.Spend(base.Owner, MainFile.RellyId, totalRelly);
        }

        // 5. 若原始充能 ≥100，移除所有敌人格挡
        if (totalRelly >= 100)
        {
            SfxCmd.Play("event:/sfx/block_break");
            foreach (var enemy in base.CombatState.Enemies.Where(e => !e.IsDead && e.Block > 0).ToList())
            {
                await CreatureCmd.LoseBlock(enemy, enemy.Block);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}