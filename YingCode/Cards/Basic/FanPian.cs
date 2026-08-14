using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class FanPian : YingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Sly]; // 奇巧

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10m, ValueProp.Move),
        new DynamicVar("Hits", 2m)
    ];

    public FanPian() : base(2, CardType.Attack, CardRarity.Common, TargetType.AllEnemies) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);
        int hits = (int)base.DynamicVars["Hits"].BaseValue;
        for (int h = 0; h < hits; h++)
        {
            foreach (var enemy in base.CombatState.HittableEnemies)
            {
                await CreatureCmd.Damage(choiceContext, enemy, base.DynamicVars.Damage.BaseValue,
                    ValueProp.Move, base.Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);
    }
}