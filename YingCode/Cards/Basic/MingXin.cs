using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Extensions; // 🌟 核心新增：引入官方原生的 UnstableShuffle 乱序扩展
using STS2RitsuLib.Interop.AutoRegistration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class MingXin : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(8m, ValueProp.Move), // 基础伤害 8
        new DynamicVar("DiscardAmount", 1m), // 基础弃牌 1
        new DynamicVar("ToTopAmount", 1m) // 基础放回抽牌堆顶数量 1
    ];

    public MingXin()
        : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) //
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target"); //

        // 1. 造成基础伤害
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        var handPile = PileType.Hand.GetPile(base.Owner); //
        if (handPile == null) return;

        // =========================================================================
        // 🌟 核心修复分流：检测是否为静滞等机制引发的【自动打出】
        // =========================================================================
        if (cardPlay.IsAutoPlay) //
        {
            // ==========================================
            // 🤖 自动打出旁路：100% 绕过 UI，使用官方种子随机结算，绝不卡死！
            // ==========================================

            // 2. 🤖 自动弃牌
            int discardCount = (int)base.DynamicVars["DiscardAmount"].BaseValue;
            var validToDiscard = handPile.Cards.Where(c => c != this).ToList(); //
            // 致敬官方：使用专用的战斗卡牌选择随机种子打乱手牌，并取出对应数量
            var autoDiscardList = validToDiscard.UnstableShuffle(base.Owner.RunState.Rng.CombatCardSelection).Take(discardCount).ToList();

            foreach (var cardToDiscard in autoDiscardList)
            {
                await CardCmd.Discard(choiceContext, cardToDiscard); //
            }

            // 3. 🤖 自动放回抽牌堆顶
            if (handPile.Cards.Any(c => c != this)) //
            {
                int toTopCount = (int)base.DynamicVars["ToTopAmount"].BaseValue; //
                var validToTop = handPile.Cards.Where(c => c != this).ToList(); //
                // 同样使用官方安全种子进行盲选
                var autoToTopList = validToTop.UnstableShuffle(base.Owner.RunState.Rng.CombatCardSelection).Take(toTopCount).ToList();

                foreach (var cardToTop in autoToTopList)
                {
                    await CardPileCmd.Add(cardToTop, PileType.Draw, CardPilePosition.Top); //
                }

                if (autoToTopList.Any())
                {
                    SfxCmd.Play("event:/sfx/ui/card_draw", 1.0f); //
                }
            }
        }
        else
        {
            // ==========================================
            // 👤 正常打出旁路：玩家手动使用，照常弹出交互选牌 UI
            // ==========================================

            // 2. 👤 玩家手动弃牌
            if (handPile.Cards.Any(c => c != this)) //
            {
                int discardCount = (int)base.DynamicVars["DiscardAmount"].BaseValue;
                CardSelectorPrefs discardPrefs = new CardSelectorPrefs(base.SelectionScreenPrompt, discardCount);

                var selectedDiscard = await CardSelectCmd.FromHand(
                    choiceContext,
                    base.Owner,
                    discardPrefs,
                    (CardModel c) => c != this,
                    this
                );

                foreach (var cardToDiscard in selectedDiscard)
                {
                    await CardCmd.Discard(choiceContext, cardToDiscard); //
                }
            }

            // 3. 👤 玩家手动放回抽牌堆顶
            if (handPile.Cards.Any(c => c != this)) //
            {
                int toTopCount = (int)base.DynamicVars["ToTopAmount"].BaseValue; //
                LocString topPrompt = new LocString("cards", base.Id.Entry + ".topScreenPrompt"); //
                CardSelectorPrefs topPrefs = new CardSelectorPrefs(topPrompt, toTopCount); //

                var selectedToTop = await CardSelectCmd.FromHand(
                    choiceContext,
                    base.Owner,
                    topPrefs,
                    (CardModel c) => c != this, //
                    this //
                );

                foreach (var cardToTop in selectedToTop)
                {
                    await CardPileCmd.Add(cardToTop, PileType.Draw, CardPilePosition.Top); //
                }

                if (selectedToTop.Any())
                {
                    SfxCmd.Play("event:/sfx/ui/card_draw", 1.0f); //
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(3m); // 保持你原本的升级数值
    }
}