using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class PanJueTwo : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(15m, ValueProp.Move),
        new DynamicVar("BonusDamage", 15m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Ethereal];

    public PanJueTwo() : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target!;

        // 造成基础伤害
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // 条件：目标有判定 debuff 且时间滞缓层数 > 当前生命
        bool hasPanJue = target.GetPower<PanJueDebuffPower>() != null;
        int slowStacks = target.GetPowerAmount<ShiJianZhiHuanPower>();

        if (hasPanJue && slowStacks > target.CurrentHp)
        {
            // 额外伤害
            await DamageCmd.Attack(base.DynamicVars["BonusDamage"].BaseValue)
                .FromCard(this)
                .Targeting(target)
                .Execute(choiceContext);

            // 获得最终判决
            var finalJudgement = base.CombatState.CreateCard<FinalPanJue>(base.Owner);
            await CardPileCmd.Add(finalJudgement, PileType.Hand);
        }
    }
}