using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class HuiGuang : YingCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Block", 5m),
        new DynamicVar("Copies", 2m)
    ];

    public HuiGuang() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars["Block"].BaseValue, ValueProp.Move, cardPlay);

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
            await CardPileCmd.Add(clone, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Block"].UpgradeValueBy(3m); // 5→8
    }
}