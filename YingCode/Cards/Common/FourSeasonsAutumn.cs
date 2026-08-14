using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;
using Yingmod.Ying.Tags;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class FourSeasonsAutumn : YingCard
{
	private decimal _extraDamage;

	protected override HashSet<CardTag> CanonicalTags => [YingCardTags.FourSeasons];
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	// 🌟 严格适配：将 ExtraHoverTips 变更为 AdditionalHoverTips 并合并基类
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
		..base.AdditionalHoverTips,
		HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
	];

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(1m, ValueProp.Move),
		new CalculationBaseVar(1m),
		new CalculationExtraVar(1m),
		new CalculatedVar("TotalHits").WithMultiplier((CardModel card, Creature? target) => {
			var discardPile = PileType.Discard.GetPile(card.Owner);
			if (discardPile?.Cards == null) return 0m;
			return (decimal)discardPile.Cards.Count((CardModel c) => c.Tags.Contains(YingCardTags.FourSeasons));
		})
	];

	private decimal ExtraDamage
	{
		get => _extraDamage;
		set { AssertMutable(); _extraDamage = value; }
	}

	public FourSeasonsAutumn() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.AnyEnemy) { }

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

		await PowerCmd.Apply<TempAutumnDexPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
		await PowerCmd.Apply<TempAutumnStrPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);

		CardPile handPile = PileType.Hand.GetPile(base.Owner);
		CardModel cardModel = base.Owner.RunState.Rng.CombatCardSelection.NextItem(handPile.Cards.Where((CardModel c) => c.Type == CardType.Attack));
		if (cardModel != null)
		{
			decimal damage = default;
			if (cardModel.DynamicVars.ContainsKey("CalculatedDamage"))
				damage = cardModel.DynamicVars.CalculatedDamage.Calculate(null);
			else if (cardModel.DynamicVars.ContainsKey("Damage"))
				damage = cardModel.DynamicVars.Damage.BaseValue;
			else if (cardModel.DynamicVars.ContainsKey("OstyDamage"))
				damage = cardModel.DynamicVars.OstyDamage.BaseValue;
			else
				Log.Warn(base.Id.Entry + " exhausted attack card " + cardModel.Id.Entry + " that did not have an appropriate damage var!");

			damage = Hook.ModifyDamage(base.Owner.RunState, base.Owner.Creature.CombatState, null, base.Owner.Creature, damage, ValueProp.Move, cardModel, ModifyDamageHookType.All, CardPreviewMode.None, out IEnumerable<MegaCrit.Sts2.Core.Models.AbstractModel> _);
			base.DynamicVars.Damage.BaseValue += damage;
			ExtraDamage += damage;
			await CardCmd.Exhaust(choiceContext, cardModel);
		}

		decimal totalHits = ((CalculatedVar)base.DynamicVars["TotalHits"]).Calculate(cardPlay.Target);

		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.WithHitCount((int)totalHits)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.Execute(choiceContext);

		await CardCmd.Exhaust(choiceContext, this);
	}

	protected override void AfterDowngraded()
	{
		base.AfterDowngraded();
		base.DynamicVars.Damage.BaseValue += ExtraDamage;
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(1m);
	}
}
