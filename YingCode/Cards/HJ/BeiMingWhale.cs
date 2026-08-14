using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Capabilities;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class BeiMingWhale : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Durability", 18m)
    ];

    public BeiMingWhale() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var manager = Owner.Character.GetOrCreateCapability<IllusionManagerCapability>();
        manager.SetOwnerPlayer(Owner);

        int durability = (int)DynamicVars["Durability"].BaseValue;
        var illusionNode = manager.CreateAndAddIllusionUI("res://Ying/Images/BeiMingWhale.png", durability);

        await PowerCmd.Apply<BeiMingWhalePower>(choiceContext, Owner.Creature, durability, Owner.Creature, this);

        var powers = Owner.Creature.GetPowerInstances<BeiMingWhalePower>();
        var power = powers.LastOrDefault();
        if (power != null)
        {
            power.IllusionNode = illusionNode;
            power.Manager = manager;
            manager.AddIllusionPower(power);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Durability"].UpgradeValueBy(8m); // 10 → 18
    }
}