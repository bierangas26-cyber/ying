using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class BeiShuiYiZhan : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("StrengthAmount", 5m)
    ];

    public BeiShuiYiZhan() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 选择一张手牌消耗（自动打出时随机）
        var hand = PileType.Hand.GetPile(base.Owner);
        CardModel? toExhaust = null;

        if (cardPlay.IsAutoPlay)
        {
            // 自动打出：随机消耗一张（不能是自身）
            toExhaust = hand.Cards
                .Where(c => c != this)
                .OrderBy(_ => System.Guid.NewGuid())
                .FirstOrDefault();
        }
        else
        {
            // 手动打出：弹出选择界面
            var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
            var selected = await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs, _ => true, this);
            toExhaust = selected.FirstOrDefault();
        }

        if (toExhaust != null)
            await CardCmd.Exhaust(choiceContext, toExhaust);

        // 增加真实力量
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature,
            base.DynamicVars["StrengthAmount"].BaseValue, base.Owner.Creature, this);

        // 施加临时力量标记（回合结束时扣回）
        await PowerCmd.Apply<LinShiLiLiangPower>(choiceContext, base.Owner.Creature,
            base.DynamicVars["StrengthAmount"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["StrengthAmount"].UpgradeValueBy(3m); // 5→8
    }
}