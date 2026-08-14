using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Capabilities;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class Expectation : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyCost", 7m),   // 供 UI 显示费用
        new DynamicVar("Durability", 3m),  // 幻境初始耐久
        new DynamicVar("MaxHeals", 5m)     // 最大回复次数（基础5）
    ];

    public Expectation() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        // 设置心脏充能费用为 7
        this.SecondaryCosts().Set(MainFile.RellyId, 7);
    }

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

        int durability = (int)DynamicVars["Durability"].BaseValue;
        int maxHeals = (int)DynamicVars["MaxHeals"].BaseValue;

        var manager = Owner.Character.GetOrCreateCapability<IllusionManagerCapability>();
        manager.SetOwnerPlayer(Owner);

        var illusionNode = manager.CreateAndAddIllusionUI("res://Ying/Images/Expectation.png", durability);

        await PowerCmd.Apply<ExpectationPower>(choiceContext, Owner.Creature, durability, Owner.Creature, this);

        var powers = Owner.Creature.GetPowerInstances<ExpectationPower>();
        var power = powers.LastOrDefault();
        if (power != null)
        {
            power.IllusionNode = illusionNode;
            power.Manager = manager;
            power.MaxHeals = maxHeals;
            manager.AddIllusionPower(power);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxHeals"].UpgradeValueBy(2m); // 5→7
    }
}