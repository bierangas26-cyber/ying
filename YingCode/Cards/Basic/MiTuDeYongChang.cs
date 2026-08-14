using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId, MainFile.RebirthPile
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class MiTuDeYongChang : YingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyCost", 11m),
        new DynamicVar("Copies", 1m)
    ];

    public MiTuDeYongChang() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        // 设置心脏充能费用为 11（升级后变为 8）
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
        var hand = PileType.Hand.GetPile(base.Owner);
        var exhaustCards = hand.Cards.Where(c => c.Keywords.Contains(CardKeyword.Exhaust) && c != this).ToList();
        if (!exhaustCards.Any()) return;

        var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
        var selected = await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs,
            c => c.Keywords.Contains(CardKeyword.Exhaust) && c != this, this);
        var target = selected.FirstOrDefault();
        if (target == null) return;

        int copies = (int)base.DynamicVars["Copies"].BaseValue;
        for (int i = 0; i < copies; i++)
        {
            var clone = target.CreateClone();
            await CardPileCmd.Add(clone, PileType.Discard);          // 第一张复制品 → 弃牌堆

            var clone2 = target.CreateClone();
            await CardPileCmd.Add(clone2, MainFile.RebirthPile);    // 第二张复制品 → 轮回堆
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["RellyCost"].UpgradeValueBy(-3m); // 11→8
        // 更新卡牌费用
        this.SecondaryCosts().Set(MainFile.RellyId, (int)base.DynamicVars["RellyCost"].BaseValue);
    }
}