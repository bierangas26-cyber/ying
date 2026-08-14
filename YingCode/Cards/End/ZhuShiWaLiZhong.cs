using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying;
using Yingmod.Ying.Character;
using Yingmod.Ying.Keywords;
using Yingmod.Ying.Patch;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class ZhuShiWaLiZhong : YingCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromKeyword(YingKeywords.JingZhi),
        HoverTipFactory.FromPower<ShiJianZhiHuanPower>(),
        StunIntent.GetStaticHoverTip()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => new List<CardKeyword> { CardKeyword.Eternal };

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RellyCost", 40m)
    ];

    public ZhuShiWaLiZhong()
        : base(3, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
    {
        this.SecondaryCosts().Set(MainFile.RellyId, 40);
    }

    protected override bool ShouldGlowGoldInternal =>
        SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId) >= 40;

    protected override bool IsPlayable
    {
        get
        {
            int currentCharge = SecondaryResourceCmd.Get(base.Owner, MainFile.RellyId);
            return currentCharge >= 40 && base.IsPlayable;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 费用自动扣除

        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        foreach (Creature hittableEnemy in base.CombatState.HittableEnemies)
        {
            await CreatureCmd.Stun(hittableEnemy);
        }

        var handPile = PileType.Hand.GetPile(base.Owner);
        int stasisCount = 0;

        if (handPile?.Cards != null)
        {
            foreach (var card in handPile.Cards)
            {
                card.AddKeyword(YingKeywords.JingZhi);
            }
            stasisCount = handPile.Cards.Count(c => c.Keywords.Contains(YingKeywords.JingZhi));
        }

        if (stasisCount > 0)
        {
            int debuffAmount = stasisCount * 8;
            foreach (Creature hittableEnemy in base.CombatState.HittableEnemies)
            {
                await PowerCmd.Apply<ShiJianZhiHuanPower>(choiceContext, hittableEnemy,
                    debuffAmount, base.Owner.Creature, this);
            }
        }

        // ★ 标记：该玩家在下一个回合开始抽牌前获取所有静滞牌
        TimeStopManager.PendingStasisRetrievalPlayerId = base.Owner.NetId;

        // ★ 开始时停
        TimeStopManager.StartTimeStop();

        // 获得额外回合
        await PowerCmd.Apply<ZhuShiWaLiExtraTurnPower>(choiceContext, base.Owner.Creature,
            1m, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}