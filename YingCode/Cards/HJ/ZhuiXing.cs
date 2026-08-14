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
public sealed class ZhuiXing : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyCost", 8m),     // 供 UI 显示费用
        new DynamicVar("Durability", 1m)     // 基础耐久1（升级后变为4）
    ];

    public ZhuiXing() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        // 设置心脏充能费用为 8
        this.SecondaryCosts().Set(MainFile.RellyId, 8);
    }

    protected override bool IsPlayable
    {
        get
        {
            int currentCharge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
            return currentCharge >= 8 && base.IsPlayable;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 费用自动扣除

        int initialDurability = (int)DynamicVars["Durability"].BaseValue;

        var manager = Owner.Character.GetOrCreateCapability<IllusionManagerCapability>();
        manager.SetOwnerPlayer(Owner);

        var illusionNode = manager.CreateAndAddIllusionUI("res://Ying/Images/ZhuiXing.png", initialDurability);

        await PowerCmd.Apply<ZhuiXingPower>(choiceContext, Owner.Creature, initialDurability, Owner.Creature, this);

        var powers = Owner.Creature.GetPowerInstances<ZhuiXingPower>();
        var power = powers.LastOrDefault();
        if (power != null)
        {
            power.IllusionNode = illusionNode;
            power.Manager = manager;
            power.InitialDurability = initialDurability;
            manager.AddIllusionPower(power);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Durability"].UpgradeValueBy(3m); // 1 → 4
    }
}