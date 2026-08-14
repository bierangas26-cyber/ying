using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class XinXueChongPei : YingCard
{
    // 🌟 严格适配：将 ExtraHoverTips 变更为 AdditionalHoverTips 并合并基类
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromPower<VigorPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Threshold", 6m)
    ];

    public XinXueChongPei()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        if (base.DynamicVars["Threshold"].BaseValue <= 4m)
        {
            await PowerCmd.Apply<XinXueChongPeiPlusPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<XinXueChongPeiPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Threshold"].UpgradeValueBy(-2m);
    }
}