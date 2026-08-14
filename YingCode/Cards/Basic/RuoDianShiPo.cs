using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class RuoDianShiPo : YingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(11m, ValueProp.Move),
        new DynamicVar("WeakAmount", 2m),
        new DynamicVar("VulnAmount", 1m),
        new DynamicVar("SlowPercent", 25m),
        new DynamicVar("RellyCost", 10m)   // 基础消耗10（供 UI 显示）
    ];

    public RuoDianShiPo() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        // 设置心脏充能费用为 10（次要资源系统自动扣除）
        this.SecondaryCosts().Set(MainFile.RellyId, (int)DynamicVars["RellyCost"].BaseValue);
    }

    protected override bool ShouldGlowGoldInternal =>
        SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId) >= base.DynamicVars["RellyCost"].BaseValue;

    protected override bool IsPlayable
    {
        get
        {
            int currentCharge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
            return currentCharge >= base.DynamicVars["RellyCost"].BaseValue && base.IsPlayable;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        // 心脏充能费用已自动扣除，无需手动操作

        // 攻击
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx(VfxCmd.slashPath)
            .Execute(choiceContext);

        // 滞缓增加25%
        var slowness = cardPlay.Target.GetPower<ShiJianZhiHuanPower>();
        if (slowness != null)
        {
            int increase = (int)(slowness.Amount * base.DynamicVars["SlowPercent"].BaseValue / 100m);
            if (increase > 0)
                await PowerCmd.Apply<ShiJianZhiHuanPower>(choiceContext, cardPlay.Target, increase, base.Owner.Creature, this);
        }

        // 施加虚弱和易伤
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, base.DynamicVars["WeakAmount"].BaseValue, base.Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, base.DynamicVars["VulnAmount"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 消耗降为6
        base.DynamicVars["RellyCost"].UpgradeValueBy(-4m);
        this.SecondaryCosts().Set(MainFile.RellyId, (int)base.DynamicVars["RellyCost"].BaseValue);
    }
}