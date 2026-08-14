using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Character;
using Yingmod.Ying.Keywords;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class ChunQiuLunZhuan : YingCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromKeyword(YingKeywords.JingZhi)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyCost", 10m)   // 现在名字是 RellyCost
    ];

    public ChunQiuLunZhuan()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        // 设置心脏充能费用为10
        this.SecondaryCosts().Set(MainFile.RellyId, 10);
    }

    protected override bool ShouldGlowGoldInternal =>
        SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId) >= 10;

    protected override bool IsPlayable
    {
        get
        {
            int currentCharge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
            return currentCharge >= 10 && base.IsPlayable;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 费用已自动扣除

        var handPile = PileType.Hand.GetPile(base.Owner);
        List<CardModel> handCards = handPile.Cards.ToList();
        int discardedCount = handCards.Count;
        if (discardedCount > 0)
        {
            await CardCmd.Discard(choiceContext, handCards);
            await CardPileCmd.Draw(choiceContext, discardedCount, base.Owner);
        }

        var deckPile = PileType.Deck.GetPile(base.Owner);
        int jingZhiCount = deckPile.Cards.Count(c => c.Keywords.Contains(YingKeywords.JingZhi));
        if (jingZhiCount > 0)
        {
            await SecondaryResourceCmd.Gain(base.Owner, MainFile.RellyId, jingZhiCount);
        }
    }

    protected override void OnUpgrade()
    {
        // 关键修正：变量名改为 RellyCost
        base.DynamicVars["RellyCost"].UpgradeValueBy(-3m); // 10 → 7
        // 更新次要资源费用
        this.SecondaryCosts().Set(MainFile.RellyId, (int)base.DynamicVars["RellyCost"].BaseValue);
    }
}