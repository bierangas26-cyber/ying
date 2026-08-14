using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class ChongNengDaJi : YingCard
{
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
		..base.AdditionalHoverTips,
		HoverTipFactory.FromPower<VulnerablePower>(),
		HoverTipFactory.FromPower<ShiJianZhiHuanPower>()
	];

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(12m, ValueProp.Move),
		new PowerVar<VulnerablePower>(1m),
		new DynamicVar("DebuffAmount", 10m),
		new DynamicVar("RellyCost", 3m)   // ★ 必须保留，供补丁显示费用
	];

	public ChongNengDaJi()
		: base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
	{
		// 设置心脏充能费用为 3
		this.SecondaryCosts().Set(MainFile.RellyId, 3);
	}

	protected override bool ShouldGlowGoldInternal =>
		SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId) >= 3;

	protected override bool IsPlayable
	{
		get
		{
			int currentCharge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
			return currentCharge >= 3 && base.IsPlayable;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		// 注意：心脏充能费用会由次要资源系统自动扣除，无需手动消耗

		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);

		// 造成伤害
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
			.Execute(choiceContext);

		// 施加时间滞缓
		await PowerCmd.Apply<ShiJianZhiHuanPower>(choiceContext, cardPlay.Target,
			base.DynamicVars["DebuffAmount"].BaseValue, base.Owner.Creature, this);

		// 施加易伤
		await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target,
			base.DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(4m);
		base.DynamicVars.Vulnerable.UpgradeValueBy(1m);
		base.DynamicVars["DebuffAmount"].UpgradeValueBy(2m);
	}
   
}
