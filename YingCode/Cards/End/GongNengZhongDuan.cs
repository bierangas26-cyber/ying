using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Cards;

[RegisterCard(typeof(YingCardPool))]
public sealed class GongNengZhongDuan : YingCard
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("BaseBlock", 15m)
    ];

    public GongNengZhongDuan() : base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 获得基础15点格挡
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars["BaseBlock"].BaseValue, ValueProp.Move, cardPlay);

        // 2. 将当前格挡值翻倍
        int currentBlock = base.Owner.Creature.Block;
        if (currentBlock > 0)
            await CreatureCmd.GainBlock(base.Owner.Creature, currentBlock, ValueProp.Move, cardPlay);

        // 3. 获得等于最终格挡值的心脏充能（次要资源系统）
        int totalBlock = base.Owner.Creature.Block;
        if (totalBlock > 0)
            await SecondaryResourceCmd.Gain(base.Owner, MainFile.RellyId, totalBlock);
    }
}