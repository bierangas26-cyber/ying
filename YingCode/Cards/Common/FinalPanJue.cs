using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class FinalPanJue : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5m, ValueProp.Move),
        new DynamicVar("Hits", 10m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Ethereal];

    public FinalPanJue() : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int hits = (int)base.DynamicVars["Hits"].BaseValue;

        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .WithHitCount(hits)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
    }
}