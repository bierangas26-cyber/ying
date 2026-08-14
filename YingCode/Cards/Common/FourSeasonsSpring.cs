using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
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
public sealed class FourSeasonsSpring : YingCard
{
	protected override HashSet<CardTag> CanonicalTags => [YingCardTags.FourSeasons];
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	// 🌟 严格适配：将 ExtraHoverTips 变更为 AdditionalHoverTips 并合并基类
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
		..base.AdditionalHoverTips,
		HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
	];

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new CalculationBaseVar(8m),
		new CalculationExtraVar(2m),
		new CalculatedVar("TotalBlock").WithMultiplier((CardModel card, Creature? target) => {
			if (CombatManager.Instance?.History?.Entries == null) return 0m;
			return (decimal)CombatManager.Instance.History.Entries
				.OfType<CardPlayFinishedEntry>()
				.Count(e => e.Actor == card.Owner.Creature && e.CardPlay.Card.Tags.Contains(YingCardTags.FourSeasons));
		})
	];

	public FourSeasonsSpring() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self) { }

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

		decimal totalBlock = ((CalculatedVar)base.DynamicVars["TotalBlock"]).Calculate(base.Owner.Creature);

		// 🌟 终极修复：严格对齐 BlockConsoleCmd.cs 的三参数格式，传入 BlockVar 包装实例和出牌环境
		await CreatureCmd.GainBlock(
			base.Owner.Creature,
			new BlockVar(totalBlock, ValueProp.Move),
			cardPlay
		);

		await CreatureCmd.Heal(base.Owner.Creature, 1m);
		await CardCmd.Exhaust(choiceContext, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.CalculationExtra.UpgradeValueBy(1m);
	}
}
