using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
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
public class YinLiDanGong : YingCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromPower<ShiJianZhiHuanPower>(),
        HoverTipFactory.FromPower<YinLiDanGongPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(14m, ValueProp.Move),
        new DynamicVar("RellyCost", 7m),   // 供 UI 显示费用用
        new PowerVar<YinLiDanGongPower>(14m)
    ];

    public YinLiDanGong()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        // 设置心脏充能费用为 7
        this.SecondaryCosts().Set(MainFile.RellyId, 7);
    }

    protected override bool ShouldGlowGoldInternal =>
        SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId) >= 7;

    protected override bool IsPlayable
    {
        get
        {
            int currentCharge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
            return currentCharge >= 7 && base.IsPlayable;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 费用自动扣除

        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);

        // 造成基础伤害
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);

        // 引爆已存在的引力弹弓 Buff
        var existingPower = base.Owner.Creature.GetPower<YinLiDanGongPower>();
        if (existingPower != null)
        {
            await PowerCmd.Apply<ShiJianZhiHuanPower>(choiceContext, cardPlay.Target, existingPower.Amount, base.Owner.Creature, this);
            await PowerCmd.Remove(existingPower);
        }

        // 挂上新的潜伏 Buff
        int amount = (int)base.DynamicVars["YinLiDanGongPower"].BaseValue;
        await PowerCmd.Apply<YinLiDanGongPower>(choiceContext, base.Owner.Creature, amount, base.Owner.Creature, this);

        var newPower = base.Owner.Creature.GetPower<YinLiDanGongPower>();
        if (newPower != null)
        {
            newPower.CreatorCard = this;
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(4m);
        base.DynamicVars["YinLiDanGongPower"].UpgradeValueBy(4m);
    }
}