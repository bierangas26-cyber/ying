using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
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
public sealed class WangXiZhiGe : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyCost", 15m),   // UI 显示用
        new DynamicVar("SelectCount", 2m)
    ];

    public WangXiZhiGe() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        // 设置心脏充能费用为 15
        this.SecondaryCosts().Set(MainFile.RellyId, 15);
    }

    protected override bool ShouldGlowGoldInternal =>
        SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId) >= 15;

    protected override bool IsPlayable
    {
        get
        {
            int charge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
            return charge >= 15 && base.IsPlayable;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 费用自动扣除

        var exhaustPile = PileType.Exhaust.GetPile(base.Owner);
        if (exhaustPile.Cards.Count == 0) return;

        int max = (int)base.DynamicVars["SelectCount"].BaseValue;
        var prefs = new CardSelectorPrefs(
            new LocString("cards", "YING_CARD_WANG_XI_ZHI_GE.select"), 0, max);
        var selected = await CardSelectCmd.FromCombatPile(choiceContext, exhaustPile, base.Owner, prefs);
        foreach (var c in selected)
            await CardCmd.AutoPlay(choiceContext, c, null);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["SelectCount"].UpgradeValueBy(1m); // 2→3
    }
}