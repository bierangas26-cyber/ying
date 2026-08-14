using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Cards;
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class SuMing : YingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyCost", 4m)   // 保留用于 UI 显示
    ];

    public SuMing() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        // 设置心脏充能费用
        this.SecondaryCosts().Set(MainFile.RellyId, (int)DynamicVars["RellyCost"].BaseValue);
    }

    protected override bool ShouldGlowGoldInternal =>
        SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId) >= base.DynamicVars["RellyCost"].BaseValue;

    protected override bool IsPlayable
    {
        get
        {
            int currentCharge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
            return currentCharge >= base.DynamicVars["RellyCost"].BaseValue && base.IsPlayable;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 心脏充能费用由次要资源系统自动扣除

        // 获取“注视瓦砾·始”的正确 ID
        ModelId targetId = ModelDb.GetId<ZhuShiWaLiShi>();

        // 在所有卡牌中查找该 ID
        var targetCards = base.Owner.PlayerCombatState.AllCards
            .Where(c => c.Id == targetId)
            .ToList();

        // 逐张移入手牌
        foreach (var card in targetCards)
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["RellyCost"].UpgradeValueBy(-2m); // 4→2
        this.SecondaryCosts().Set(MainFile.RellyId, (int)base.DynamicVars["RellyCost"].BaseValue);
    }
}