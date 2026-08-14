using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class JiTongChengFeng : YingCard
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyPerX", 3m)    // 每点能量获得的充能
    ];

    public JiTongChengFeng() : base(-1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int x = EnergyCost.CapturedXValue;
        if (x <= 0) return;

        var hand = PileType.Hand.GetPile(base.Owner);
        var eligible = hand.Cards.Where(c => !c.Keywords.Contains(CardKeyword.Eternal)).ToList();
        int select = Math.Min(x, eligible.Count);

        if (select > 0)
        {
            var prefs = new CardSelectorPrefs(
                new LocString("cards", "YING_CARD_JI_TONG_CHENG_FENG.select"), select, select);
            var selected = await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs,
                c => !c.Keywords.Contains(CardKeyword.Eternal), this);

            foreach (var card in selected)
            {
                CardCmd.ApplyKeyword(card, CardKeyword.Exhaust);
                // 使用官方费用修饰器，本回合免费
                card.EnergyCost.SetThisTurnOrUntilPlayed(0);
            }
        }

        // 根据消耗的能量 X 给予力量与心脏充能
        int strengthGain = x;
        int rellyGain = x * (int)base.DynamicVars["RellyPerX"].BaseValue;

        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, strengthGain, base.Owner.Creature, this);
        // 次要资源系统：获得心脏充能
        await SecondaryResourceCmd.Gain(base.Owner, MainFile.RellyId, rellyGain);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["RellyPerX"].UpgradeValueBy(2m); // 3→5
    }
}