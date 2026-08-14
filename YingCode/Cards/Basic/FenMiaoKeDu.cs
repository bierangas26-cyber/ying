using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
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
public sealed class FenMiaoKeDu : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyGain", 4m),
        new DynamicVar("Copies", 2m)
    ];

    public FenMiaoKeDu() : base(2, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得心脏充能
        await SecondaryResourceCmd.Gain(base.Owner, MainFile.RellyId,
            (int)base.DynamicVars["RellyGain"].BaseValue);

        var hand = PileType.Hand.GetPile(base.Owner);
        var exhaustCards = hand.Cards.Where(c => c.Keywords.Contains(CardKeyword.Exhaust) && c != this).ToList();
        if (!exhaustCards.Any()) return;

        var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
        var selected = await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs,
            c => c.Keywords.Contains(CardKeyword.Exhaust) && c != this, this);
        var target = selected.FirstOrDefault();
        if (target == null) return;

        int copies = (int)base.DynamicVars["Copies"].BaseValue;
        for (int i = 0; i < copies; i++)
        {
            var clone = target.CreateClone();
            await CardPileCmd.Add(clone, PileType.Draw);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级：心脏充能获取 +4 (4→8)
        base.DynamicVars["RellyGain"].UpgradeValueBy(4m);
        // 能量费用减 1 (2→1)
        EnergyCost.SetThisCombat(1);
    }
}