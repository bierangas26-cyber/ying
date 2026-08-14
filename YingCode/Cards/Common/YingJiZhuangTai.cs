using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;
using Yingmod.Ying.Cards;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class YingJiZhuangTai : YingCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromCard<JinJiGongNeng>()
    ];

    public YingJiZhuangTai()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        // 层数决定每回合生成的张数：基础 1 层，升级后 2 层
        int amount = base.IsUpgraded ? 2 : 1;
        await PowerCmd.Apply<YingJiZhuangTaiPower>(choiceContext, base.Owner.Creature, amount, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级效果仅通过层数体现，无需额外操作
    }
}