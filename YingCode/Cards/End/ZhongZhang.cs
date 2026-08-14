using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
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
public sealed class ZhongZhang : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyCost", 18m),   // UI 显示用
        new DynamicVar("DrawCount", 2m)
    ];

    public ZhongZhang() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        // 设置心脏充能费用为 18
        this.SecondaryCosts().Set(MainFile.RellyId, 18);
    }

    protected override bool ShouldGlowGoldInternal =>
        SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId) >= 18;

    protected override bool IsPlayable
    {
        get
        {
            int charge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
            return charge >= 18 && base.IsPlayable;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 费用自动扣除

        var power = await PowerCmd.Apply<ZhongZhangPower>(choiceContext, base.Owner.Creature,
            1, base.Owner.Creature, this);
        if (power != null)
        {
            power.DrawCount = (int)base.DynamicVars["DrawCount"].BaseValue;
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["DrawCount"].UpgradeValueBy(1m); // 2→3
    }
}