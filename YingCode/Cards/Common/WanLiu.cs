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
public sealed class WanLiu : YingCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Block", 14m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<ShiJianZhiHuanPower>();
            yield return HoverTipFactory.Static(StaticHoverTip.Block);
        }
    }

    public WanLiu() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得格挡
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars["Block"].BaseValue, ValueProp.Move, cardPlay);

        // 施加挽留能力，并记录日志
        var power = await PowerCmd.Apply<WanLiuPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
        Yingmod.Ying.MainFile.Logger.Info($"[WanLiu] Power applied: {power != null}");
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Block"].UpgradeValueBy(4m); // 14 → 18
    }
}