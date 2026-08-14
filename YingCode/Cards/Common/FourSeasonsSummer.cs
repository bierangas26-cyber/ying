using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Tags;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class FourSeasonsSummer : YingCard
{
    protected override HashSet<CardTag> CanonicalTags => [YingCardTags.FourSeasons];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    // 🌟 严格适配：将 ExtraHoverTips 变更为 AdditionalHoverTips 并合并基类
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(3m),
        new CalculationExtraVar(3m),
        new CalculatedVar("TotalHpLoss").WithMultiplier((CardModel card, Creature? target) => {
            var exhaustPile = PileType.Exhaust.GetPile(card.Owner);
            if (exhaustPile?.Cards == null) return 0m;
            return (decimal)exhaustPile.Cards.Count((CardModel c) => c.Tags.Contains(YingCardTags.FourSeasons));
        })
    ];

    public FourSeasonsSummer() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        decimal totalHpLoss = ((CalculatedVar)base.DynamicVars["TotalHpLoss"]).Calculate(cardPlay.Target);

        VfxCmd.PlayOnCreature(cardPlay.Target, VfxCmd.bluntPath);
        await Cmd.Wait(0.1f);

        await CreatureCmd.Damage(choiceContext, cardPlay.Target, totalHpLoss, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, base.Owner.Creature, this);
        await CardCmd.Exhaust(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.CalculationExtra.UpgradeValueBy(1m);
    }
}