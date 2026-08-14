using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class BoDongDeXin : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyCost", 10m),   // UI 显示用
        new DynamicVar("CopyCount", 1m)
    ];

    public BoDongDeXin() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        this.SecondaryCosts().Set(MainFile.RellyId, 10);
    }

    protected override bool ShouldGlowGoldInternal =>
        SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId) >= 10;

    protected override bool IsPlayable
    {
        get
        {
            int charge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
            return charge >= 10 && base.IsPlayable;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 费用由次要资源系统自动扣除

        var hand = PileType.Hand.GetPile(base.Owner);
        var exhaustCards = hand.Cards.Where(c => c.Keywords.Contains(CardKeyword.Exhaust)).ToList();
        int copies = (int)base.DynamicVars["CopyCount"].BaseValue;
        foreach (var card in exhaustCards)
        {
            for (int i = 0; i < copies; i++)
            {
                var clone = card.CreateClone();
                await CardPileCmd.Add(clone, PileType.Discard);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["CopyCount"].UpgradeValueBy(1m); // 1→2
    }
}