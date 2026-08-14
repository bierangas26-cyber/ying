using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Tags;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class FourSeasonsWinter : YingCard
{
    protected override HashSet<CardTag> CanonicalTags => [YingCardTags.FourSeasons];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    // 🌟 严格适配：将 ExtraHoverTips 变更为 AdditionalHoverTips 并合并基类
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new SummonVar(8m),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("TotalWeak").WithMultiplier((CardModel card, Creature? target) => {
            if (card.Owner?.PlayerCombatState?.AllCards == null) return 0m;
            return (decimal)card.Owner.PlayerCombatState.AllCards.Count((CardModel c) => c.Tags.Contains(YingCardTags.FourSeasons));
        })
    ];

    public FourSeasonsWinter() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        var aliveEnemies = base.CombatState.Enemies.Where(m => m.IsAlive).ToList();
        if (aliveEnemies.Count > 0)
        {
            Creature randomTarget = base.Owner.RunState.Rng.CombatCardSelection.NextItem(aliveEnemies);

            decimal totalWeak = ((CalculatedVar)base.DynamicVars["TotalWeak"]).Calculate(randomTarget);

            if (totalWeak > 0)
            {
                await PowerCmd.Apply<WeakPower>(choiceContext, randomTarget, totalWeak, base.Owner.Creature, this);
            }
        }

        await OstyCmd.Summon(choiceContext, base.Owner, base.DynamicVars["Summon"].BaseValue, this);
        await CardCmd.Exhaust(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Summon"].UpgradeValueBy(7m);
    }
}