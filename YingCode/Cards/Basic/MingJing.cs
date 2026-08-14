using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Cards;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class MingJing : YingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Block", 8m)
    ];

    public MingJing() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得格挡
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars["Block"].BaseValue, ValueProp.Move, cardPlay);

        // 手牌中除自己以外可选的牌
        var hand = PileType.Hand.GetPile(base.Owner);
        var eligible = hand.Cards.Where(c => c != this).ToList();
        if (eligible.Count == 0) return;

        // 判断是否为自动打出（静滞、终末等）
        if (cardPlay.IsAutoPlay)
        {
            // 自动打出时：随机选择两张手牌变化为紧急供能（不弹窗）
            int selectCount = Math.Min(2, eligible.Count);
            var rng = base.Owner.RunState.Rng.CombatCardSelection;
            for (int i = 0; i < selectCount; i++)
            {
                var randomCard = rng.NextItem(eligible);
                if (randomCard != null)
                {
                    await CardCmd.TransformTo<JinJiGongNeng>(randomCard);
                    eligible.Remove(randomCard);
                }
            }
        }
        else
        {
            // 手动打出时：弹出选牌窗口，选择0~2张牌变化
            var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 0, 2);
            var selected = await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs, _ => true, this);
            foreach (var card in selected)
            {
                await CardCmd.TransformTo<JinJiGongNeng>(card);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Block"].UpgradeValueBy(3m); // 8 → 11
    }
}