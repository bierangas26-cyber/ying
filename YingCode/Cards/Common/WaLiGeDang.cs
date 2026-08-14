using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class WaLiGeDang : YingCard
{
    public override bool GainsBlock => true;
    protected override PileType GetResultPileTypeForCardPlay() => PileType.None;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyCost", 12m),   // UI 显示用
        new DynamicVar("Block", 16m)
    ];

    public WaLiGeDang() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        // 设置心脏充能费用为 12
        this.SecondaryCosts().Set(MainFile.RellyId, 12);
    }

    protected override bool ShouldGlowGoldInternal =>
        SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId) >= 12;

    protected override bool IsPlayable
    {
        get
        {
            int charge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
            return charge >= 12 && base.IsPlayable;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 费用自动扣除

        await CreatureCmd.GainBlock(base.Owner.Creature,
            base.DynamicVars["Block"].BaseValue, ValueProp.Move, cardPlay);

        // 返回手牌
        await CardPileCmd.Add(this, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Block"].UpgradeValueBy(4m); // 16→20
    }
}