using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
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
public sealed class Shiyi : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyCost", 6m)   // UI 显示用费用
    ];

    public Shiyi() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        // 设置心脏充能费用为 6
        this.SecondaryCosts().Set(MainFile.RellyId, 6);
    }

    protected override bool IsPlayable
    {
        get
        {
            int currentCharge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
            if (currentCharge < 6) return false;
            var exhaustPile = PileType.Exhaust.GetPile(base.Owner);
            return exhaustPile != null && exhaustPile.Cards.Count >= 2 && base.IsPlayable;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 费用自动扣除

        var exhaustPile = PileType.Exhaust.GetPile(base.Owner);
        if (exhaustPile == null || exhaustPile.Cards.Count < 2) return;

        int selectCount = base.IsUpgraded ? 3 : 2;

        var prefs = new CardSelectorPrefs(
            new LocString("cards", "YING_CARD_SHIYI.selectionScreenPrompt"),
            selectCount,
            selectCount
        );

        var selectedCards = await CardSelectCmd.FromCombatPile(
            choiceContext,
            exhaustPile,
            base.Owner,
            prefs
        );

        if (selectedCards == null || !selectedCards.Any()) return;

        foreach (var card in selectedCards)
        {
            await CardPileCmd.Add(card, PileType.Hand);
            card.EnergyCost.SetThisTurnOrUntilPlayed(0);
        }
    }

    protected override void OnUpgrade() { }
}