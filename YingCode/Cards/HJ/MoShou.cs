using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Capabilities;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class MoShou : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(14m, ValueProp.Move),
        new DynamicVar("Durability", 10m)
    ];

    public MoShou() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var blockVar = (BlockVar)DynamicVars["Block"];
        await CreatureCmd.GainBlock(Owner.Creature, blockVar, null);

        int durability = (int)DynamicVars["Durability"].BaseValue;

        // 获取管理器，绑定玩家
        var manager = Owner.Character.GetOrCreateCapability<IllusionManagerCapability>();
        manager.SetOwnerPlayer(Owner);

        // 创建幻境 UI
        var illusionNode = manager.CreateAndAddIllusionUI("res://Ying/Images/MoShou.png", durability);

        // 施加独立幻境能力
        await PowerCmd.Apply<MoShouIllusionPower>(choiceContext, Owner.Creature, durability, Owner.Creature, this);

        // 获取最新实例
        var powers = Owner.Creature.GetPowerInstances<MoShouIllusionPower>();
        var power = powers.LastOrDefault();
        if (power != null)
        {
            power.IllusionNode = illusionNode;
            power.Manager = manager;
            manager.AddIllusionPower(power);
        }
    }

    protected override void OnUpgrade() => DynamicVars["Block"].UpgradeValueBy(6m);
}