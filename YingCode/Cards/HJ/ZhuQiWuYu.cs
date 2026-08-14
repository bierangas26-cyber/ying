using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using System.Threading.Tasks;
using Yingmod.Ying.Capabilities;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class ZhuQiWuYu : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Durability", 10m)
    ];

    public ZhuQiWuYu() : base(2, CardType.Power, CardRarity.Ancient, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 获取幻境管理器
        var manager = base.Owner.Character.GetOrCreateCapability<IllusionManagerCapability>();
        manager.SetOwnerPlayer(base.Owner);

        // 2. 创建竹取物语的幻境 UI（现在只传路径和耐久）
        int durability = (int)base.DynamicVars["Durability"].BaseValue;
        manager.CreateAndAddIllusionUI("res://Ying/Images/ZhuQiWuYu.png", durability);

        // 3. 仍然给玩家施加发牌能力，只是不再需要传递 UI 节点
        await PowerCmd.Apply<ZhuQiWuYuPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
    }
}