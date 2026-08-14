using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class YuXiang : YingCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromPower<WeakPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(9m, ValueProp.Move),
        new DynamicVar("WeakAmount", 1m)
    ];

    public YuXiang()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    // 🌟 在手牌中时，实时检测是否满足连击条件（满足则亮金边！）
    protected override bool ShouldGlowGoldInternal => IsComboSatisfied(null);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);

        // 1. 先执行群体 AOE 伤害
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(base.CombatState)
            .WithHitFx("vfx/vfx_attack_slash", null, null)
            .Execute(choiceContext);

        // 2. 🌟 结算连击：把当前的动作(cardPlay)传进去，精准避开自己
        if (IsComboSatisfied(cardPlay))
        {
            // 使用官方直接给群体挂 Debuff 的原生重载方法，最稳定
            await PowerCmd.Apply<WeakPower>(choiceContext, base.CombatState.HittableEnemies, base.DynamicVars["WeakAmount"].BaseValue, base.Owner.Creature, this);
        }
    }

    // 🌟 终极防克隆黑魔法：通过比对动作实例(CardPlay)而不是卡牌本身来判定
    private bool IsComboSatisfied(CardPlay currentPlay)
    {
        if (CombatManager.Instance == null || base.CombatState == null) return false;

        // 抓取本回合所有已开始的出牌记录
        var entries = CombatManager.Instance.History.Entries
                             .OfType<CardPlayStartedEntry>()
                             .Where(e => e.HappenedThisTurn(base.CombatState))
                             .ToList();

        if (entries.Count == 0) return false;

        var lastEntry = entries.Last();

        // 如果我们正在结算这张牌（OnPlay阶段），那历史最后一条必定是当前正在打出的这张牌
        if (currentPlay != null && lastEntry.CardPlay == currentPlay)
        {
            // 我们需要看它的“上一张”，也就是倒数第二条记录
            if (entries.Count >= 2)
            {
                return entries[entries.Count - 2].CardPlay.Card.Type == CardType.Attack;
            }
            return false;
        }

        // 如果还没打出（在手牌里看它发不发光），那最后一条记录就是真正的上一张牌
        return lastEntry.CardPlay.Card.Type == CardType.Attack;
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m); // 伤害 9 -> 11
    }
}