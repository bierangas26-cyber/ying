using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class YongGu : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<YongGuPower>(2m),         // 基础给 2 层永固能力
        new DynamicVar("RellyCost", 14m)       // ★ 修正为 RellyCost，供补丁显示费用
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromPower<YongGuPower>()
    ];

    public YongGu()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        // 设置心脏充能费用为 14
        this.SecondaryCosts().Set(MainFile.RellyId, 14);
    }

    protected override bool ShouldGlowGoldInternal =>
        SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId) >= 14;

    protected override bool IsPlayable
    {
        get
        {
            int currentCharge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
            return currentCharge >= 14 && base.IsPlayable;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 费用自动扣除

        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Buff", base.Owner.Character.CastAnimDelay);

        // 给自己挂上“永固”能力
        await PowerCmd.Apply<YongGuPower>(choiceContext, base.Owner.Creature,
            base.DynamicVars["YongGuPower"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["YongGuPower"].UpgradeValueBy(1m);
    }
}