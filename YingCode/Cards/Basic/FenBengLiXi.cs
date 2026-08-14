using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class FenBengLiXi : YingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(0m, ValueProp.Move)
    ];

    public FenBengLiXi() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override bool ShouldGlowGoldInternal =>
        SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId) > 0;

    protected override bool IsPlayable
    {
        get
        {
            int currentCharge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
            return currentCharge > 0 && base.IsPlayable;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        // 获取当前心脏充能数量
        int currentCharge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
        if (currentCharge == 0) return;

        // 造成等同于充能数值的伤害
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, currentCharge, ValueProp.Move, base.Owner.Creature, this);

        // 消耗 1/4 的充能（向下取整）
        int toSpend = currentCharge / 4;
        if (toSpend > 0)
            await SecondaryResourceCmd.Spend(base.Owner, MainFile.RellyId, toSpend);
    }

    protected override void OnUpgrade()
    {
        // 升级：本场战斗免费打出
        this.SecondaryCosts().Set(MainFile.RellyId, SecondaryResourceCost.Free, SecondaryResourceCostDuration.ThisCombat);
    }
}