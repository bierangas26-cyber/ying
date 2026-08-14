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
public sealed class HeiLieChe : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("HpCost", 1m),       // 原“扣除生命上限”改为“扣除生命”
        new DynamicVar("CopyCount", 1m)
    ];

    public HeiLieChe() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await PowerCmd.Apply<HeiLieChePower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
        if (power != null)
        {
            power.HpCost = (int)base.DynamicVars["HpCost"].BaseValue;
            power.CopyCount = (int)base.DynamicVars["CopyCount"].BaseValue;
        }
    }

    protected override void OnUpgrade()
    {
        // 升级后扣血减少（假设原扣血1，升级后不扣血？可按需求调整）
        base.DynamicVars["HpCost"].UpgradeValueBy(-1m); // 1→0（不再扣血）
    }
}