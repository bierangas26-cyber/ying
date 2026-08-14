using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class SiLie : YingCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(11m, ValueProp.Move)
    ];

    public SiLie() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);

        var results = await CreatureCmd.Damage(choiceContext, cardPlay.Target, base.DynamicVars.Damage, base.Owner.Creature, this);
        decimal totalDamage = results.Sum(r => r.UnblockedDamage + r.OverkillDamage);

        if (totalDamage > 0)
        {
            await PowerCmd.Apply<ShiJianZhiHuanPower>(choiceContext, cardPlay.Target, totalDamage, base.Owner.Creature, this);
        }
        VfxCmd.PlayOnCreatureCenter(cardPlay.Target, VfxCmd.slashPath);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m); // 11->13
    }
}