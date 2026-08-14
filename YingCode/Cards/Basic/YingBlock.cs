using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public class YingBlock : YingCard
{
    public YingBlock() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 🌟 严格对齐官方正统底层格挡指令：彻底废除 CommonActions.CardBlock
        // 完美传入：玩家生物实体、动态格挡变量、以及从 OnPlay 获取的当前环境 cardPlay 动作实例，100% 安全！
        await CreatureCmd.GainBlock(
            base.Owner.Creature,
            new BlockVar(base.DynamicVars["Block"].BaseValue, ValueProp.Move),
            cardPlay
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}