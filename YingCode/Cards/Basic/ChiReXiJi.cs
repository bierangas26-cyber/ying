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
public sealed class ChiReXiJi : YingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyGain", 13m),
        new DynamicVar("SelectCount", 2m)
    ];

    public ChiReXiJi() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得 13 点心脏充能（已由次要资源系统管理，无需 RellyPower）
        await SecondaryResourceCmd.Gain(base.Owner, MainFile.RellyId,
            (int)base.DynamicVars["RellyGain"].BaseValue);

        var hand = PileType.Hand.GetPile(base.Owner);
        var nonEternal = hand.Cards.Where(c => !c.Keywords.Contains(CardKeyword.Eternal) && c != this).ToList();
        if (!nonEternal.Any()) return;

        int count = (int)base.DynamicVars["SelectCount"].BaseValue;
        var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, count);
        var selected = await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs,
            c => !c.Keywords.Contains(CardKeyword.Eternal) && c != this, this);
        foreach (var card in selected)
        {
            CardCmd.ApplyKeyword(card, CardKeyword.Exhaust);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["RellyGain"].UpgradeValueBy(3m);
    }
}