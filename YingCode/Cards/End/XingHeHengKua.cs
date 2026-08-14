using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class XingHeHengKua : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1),               // 能量图标，升级后1→2
        new DynamicVar("RellyGain", 10m) // 心脏充能
    ];

    public XingHeHengKua() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await PowerCmd.Apply<XingHeHengKuaPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
        if (power != null)
        {
            power.EnergyGain = (int)base.DynamicVars.Energy.BaseValue;
            power.RellyGain = (int)base.DynamicVars["RellyGain"].BaseValue;
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Energy.UpgradeValueBy(1);        // 1→2
        base.DynamicVars["RellyGain"].UpgradeValueBy(2m); // 10→12
    }
}