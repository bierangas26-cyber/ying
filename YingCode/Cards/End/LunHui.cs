using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Patches;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class LunHui : YingCard
{
    // 固有、永恒、保留
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Innate, CardKeyword.Eternal, CardKeyword.Retain];

    public LunHui() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int season = SeasonCyclePatch.GetCurrentSeason(base.Owner);
        if (season == 0) return;

        var player = base.Owner;
        var rebirthPile = MainFile.RebirthPile.GetPile(player);
        if (rebirthPile == null) return;

        switch (season)
        {
            case 1: await Spring(choiceContext, player, rebirthPile); break;
            case 2: await Summer(choiceContext, player, rebirthPile); break;
            case 3: await Autumn(choiceContext, player, rebirthPile); break;
            case 4: await Winter(choiceContext, player, rebirthPile); break;
        }
    }

    private async Task Spring(PlayerChoiceContext context, Player player, CardPile rebirthPile)
    {
        var prefs = new CardSelectorPrefs(
            new LocString("cards", "YING_CARD_LUN_HUI.springSelectionScreenPrompt"), 0, 4);
        var selected = await CardSelectCmd.FromHand(context, player, prefs, _ => true, this);
        if (selected.Any()) await CardPileCmd.Add(selected, rebirthPile);
    }

    private async Task Summer(PlayerChoiceContext context, Player player, CardPile rebirthPile)
    {
        var handPrefs = new CardSelectorPrefs(
            new LocString("cards", "YING_CARD_LUN_HUI.summerHandSelect"), 0, 3);
        var handSelected = await CardSelectCmd.FromHand(context, player, handPrefs, _ => true, this);
        if (handSelected.Any()) await CardPileCmd.Add(handSelected, rebirthPile);

        if (rebirthPile.Cards.Count > 0)
        {
            var pilePrefs = new CardSelectorPrefs(
                new LocString("cards", "YING_CARD_LUN_HUI.summerPileSelect"), 0, 1);
            var pileSelected = await CardSelectCmd.FromCombatPile(context, rebirthPile, player, pilePrefs);
            if (pileSelected.Any()) await CardPileCmd.Add(pileSelected, PileType.Hand);
        }
    }

    private async Task Autumn(PlayerChoiceContext context, Player player, CardPile rebirthPile)
    {
        var handPrefs = new CardSelectorPrefs(
            new LocString("cards", "YING_CARD_LUN_HUI.autumnHandSelect"), 0, 2);
        var handSelected = await CardSelectCmd.FromHand(context, player, handPrefs, _ => true, this);
        if (handSelected.Any()) await CardPileCmd.Add(handSelected, rebirthPile);

        if (rebirthPile.Cards.Count > 0)
        {
            var pilePrefs = new CardSelectorPrefs(
                new LocString("cards", "YING_CARD_LUN_HUI.autumnPileSelect"), 0, 2);
            var pileSelected = await CardSelectCmd.FromCombatPile(context, rebirthPile, player, pilePrefs);
            if (pileSelected.Any()) await CardPileCmd.Add(pileSelected, PileType.Hand);
        }
    }

    private async Task Winter(PlayerChoiceContext context, Player player, CardPile rebirthPile)
    {
        if (rebirthPile.Cards.Count == 0) return;

        var prefs = new CardSelectorPrefs(
            new LocString("cards", "YING_CARD_LUN_HUI.winterPileSelect"), 0, 3);
        var selected = await CardSelectCmd.FromCombatPile(context, rebirthPile, player, prefs);
        if (selected.Any()) await CardPileCmd.Add(selected, PileType.Hand);
    }
}
