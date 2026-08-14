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
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class RongGuang : YingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(13m, ValueProp.Move),
        new DynamicVar("SlowAmount", 8m),
        new DynamicVar("SelfCopies", 1m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ..base.AdditionalHoverTips,
        HoverTipFactory.FromPower<ShiJianZhiHuanPower>()
    ];

    public RongGuang() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx(VfxCmd.slashPath)
            .Execute(choiceContext);
        await PowerCmd.Apply<ShiJianZhiHuanPower>(choiceContext, cardPlay.Target,
            base.DynamicVars["SlowAmount"].BaseValue, base.Owner.Creature, this);

        int copies = (int)base.DynamicVars["SelfCopies"].BaseValue;
        for (int i = 0; i < copies; i++)
        {
            var clone = this.CreateClone();
            await CardPileCmd.Add(clone, PileType.Discard);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["SelfCopies"].UpgradeValueBy(1m); // 1→2
    }
}