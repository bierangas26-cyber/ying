using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Keywords;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class YongJiuLvTu : YingCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(YingKeywords.ZhongMo)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        YingKeywords.ZhongMo
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(PermanentDamage, ValueProp.Move),
        new BlockVar(PermanentBlock, ValueProp.Move),
        new IntVar("Increase", 1m)
    ];

    // =========================================================================
    // 永久成长数值（跨战斗保存）
    // =========================================================================
    private int _currentDamage = 5;
    private int _increasedDamage;
    private int _currentBlock = 5;
    private int _increasedBlock;

    [SavedProperty]
    public int PermanentDamage
    {
        get => _currentDamage;
        set
        {
            AssertMutable();
            _currentDamage = value;
            if (base.DynamicVars?.Damage != null)
            {
                base.DynamicVars.Damage.BaseValue = _currentDamage;
            }
        }
    }

    [SavedProperty]
    public int PermanentBlock
    {
        get => _currentBlock;
        set
        {
            AssertMutable();
            _currentBlock = value;
            if (base.DynamicVars?.Block != null)
            {
                base.DynamicVars.Block.BaseValue = _currentBlock;
            }
        }
    }

    [SavedProperty]
    public int PermanentIncDamage
    {
        get => _increasedDamage;
        set { AssertMutable(); _increasedDamage = value; }
    }

    [SavedProperty]
    public int PermanentIncBlock
    {
        get => _increasedBlock;
        set { AssertMutable(); _increasedBlock = value; }
    }
    // =========================================================================

    public YongJiuLvTu()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        // 播放角色攻击动画
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);

        // 1. 获得格挡
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);

        // 2. 攻击 － 使用官方确保存在的特效路径（重击特效）
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx(VfxCmd.heavyBluntPath)   // 替代原来的 vfx/vfx_attack_heavy
            .Execute(choiceContext);

        // 3. 永久成长
        int intValue = base.DynamicVars["Increase"].IntValue;
        BuffFromPlay(intValue);
        (base.DeckVersion as YongJiuLvTu)?.BuffFromPlay(intValue);
    }

    private void BuffFromPlay(int amount)
    {
        PermanentDamage += amount;
        PermanentIncDamage += amount;
        PermanentBlock += amount;
        PermanentIncBlock += amount;
    }

    protected override void OnUpgrade()
    {
        // 升级：每次打出成长值从1变为2
        base.DynamicVars["Increase"].UpgradeValueBy(1m);
    }
}