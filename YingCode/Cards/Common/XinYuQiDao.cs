using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying;
using Yingmod.Ying.Character;
using Yingmod.Ying.Keywords;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class XinYuQiDao : YingCard
{
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
		..base.AdditionalHoverTips,
		HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
		HoverTipFactory.FromKeyword(YingKeywords.ZhongMo)
	];

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new HpLossVar(1m)
	];

	public XinYuQiDao()
		: base(0, CardType.Skill, CardRarity.Basic, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

		// 扣除 1 点生命
		await CreatureCmd.Damage(choiceContext, base.Owner.Creature,
			base.DynamicVars["HpLoss"].BaseValue,
			ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this);

		var handPile = PileType.Hand.GetPile(base.Owner);

		// 筛选消耗牌（排除自身）
		var exhaustCards = handPile.Cards
			.Where(c => c != this && c.Keywords.Contains(CardKeyword.Exhaust))
			.ToList();

		if (exhaustCards.Count > 0)
		{
			// 分支 1：有消耗牌 → 选择一张，附加终末，费用变为0，复制到轮回堆 （不自动打出）
			var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
			var selected = (await CardSelectCmd.FromHand(
				choiceContext, base.Owner, prefs,
				c => c != this && c.Keywords.Contains(CardKeyword.Exhaust), this
			)).FirstOrDefault();

			if (selected != null)
			{
				// 附加终末词条
				if (!selected.Keywords.Contains(YingKeywords.ZhongMo))
					selected.AddKeyword(YingKeywords.ZhongMo);

				// 将能量费用设为0，直到被使用或回合结束（官方API，联机基本安全）
				selected.EnergyCost.SetThisTurnOrUntilPlayed(0);

				// 将一张复制品加入轮回堆
				var clone = selected.CreateClone();
				await CardPileCmd.Add(clone, MainFile.RebirthPile);

				SfxCmd.Play("event:/sfx/ui/card_upgrade", 1.0f);
			}
		}
		else
		{
			// 分支 2：无消耗牌 → 选择任意牌，免费自动打出一次（能量+充能全免）
			var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
			var selected = (await CardSelectCmd.FromHand(
				choiceContext, base.Owner, prefs,
				c => c != this, this
			)).FirstOrDefault();

			if (selected != null)
			{
				// 清零充能消耗变量
				var savedVars = new List<(DynamicVar var, decimal oldValue)>();
				foreach (var dv in selected.DynamicVars.Values)
				{
					if (dv.Name is "RellyPower" or "RellyCost" or "CostAmount")
					{
						savedVars.Add((dv, dv.BaseValue));
						dv.BaseValue = 0;
					}
				}

				try
				{
					// AutoPlay 本身不消耗能量，充能变量也已清零
					await CardCmd.AutoPlay(choiceContext, selected, null);
				}
				finally
				{
					// 恢复充能变量
					foreach (var (dv, oldValue) in savedVars)
						dv.BaseValue = oldValue;
				}

				SfxCmd.Play("event:/sfx/ui/card_upgrade", 1.0f);
			}
		}
	}

	protected override void OnUpgrade()
	{
		AddKeyword(CardKeyword.Retain);
	}
}
