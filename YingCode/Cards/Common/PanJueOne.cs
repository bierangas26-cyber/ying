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
public class PanJueOne : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10m, ValueProp.Move),
        new DynamicVar("SlowAmount", 10m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Ethereal];

    public PanJueOne() : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target!;

        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // 条件：目标有判定 debuff 且生命 > 70%
        bool hasPanJue = target.GetPower<PanJueDebuffPower>() != null;
        bool hpAbove70 = (decimal)target.CurrentHp > (decimal)target.MaxHp * 0.7m;

        if (hasPanJue && hpAbove70)
        {
            // 施加时间滞缓
            await PowerCmd.Apply<ShiJianZhiHuanPower>(choiceContext, target,
                base.DynamicVars["SlowAmount"].BaseValue, base.Owner.Creature, this);

            // 获得判决:二
            var panJueTwo = base.CombatState.CreateCard<PanJueTwo>(base.Owner);
            await CardPileCmd.Add(panJueTwo, PileType.Hand);
        }
    }
}