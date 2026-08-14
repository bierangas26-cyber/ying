using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class ZhongShenPanJue : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyCost", 10m)
    ];

    public ZhongShenPanJue()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        // 基础充能消耗 10
        this.SecondaryCosts().Set(MainFile.RellyId, 10);
    }

    protected override bool ShouldGlowGoldInternal =>
        SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId) >=
            (int)base.DynamicVars["RellyCost"].BaseValue;

    protected override bool IsPlayable
    {
        get
        {
            int currentCharge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
            return currentCharge >= (int)base.DynamicVars["RellyCost"].BaseValue && base.IsPlayable;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 施加“判定”
        await PowerCmd.Apply<PanJueDebuffPower>(choiceContext, cardPlay.Target!, 1, base.Owner.Creature, this);

        // 创建判决:一 并加入手牌
        var panJueOne = base.CombatState.CreateCard<PanJueOne>(base.Owner);
        await CardPileCmd.Add(panJueOne, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        // 降低充能消耗：10 → 5
        this.SecondaryCosts().Set(MainFile.RellyId, 5);
        base.DynamicVars["RellyCost"].UpgradeValueBy(-5m);
    }
}