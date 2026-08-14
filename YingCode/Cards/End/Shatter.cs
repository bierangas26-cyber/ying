using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using System.Threading.Tasks;
using Yingmod.Ying.Capabilities;
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class Shatter : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(16m, ValueProp.Move),
        new DynamicVar("Block", 5m)
    ];

    public Shatter() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        var manager = Owner.Character.GetOrCreateCapability<IllusionManagerCapability>();
        if (manager == null) return;

        // 逐个销毁所有幻境
        while (manager.Illusions.Count > 0)
        {
            var illusion = manager.Illusions[0];
            illusion.Break();

            // 造成伤害
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .WithHitFx(VfxCmd.bluntPath)
                .Execute(choiceContext);

            // 获得格挡
            int block = (int)base.DynamicVars["Block"].BaseValue;
            await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, null);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(4m); // 16 → 20
    }
}