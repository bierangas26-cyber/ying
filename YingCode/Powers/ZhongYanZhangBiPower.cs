using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;
using Yingmod.Ying.Cards;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class ZhongYanZhangBiPower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != base.Owner) return;
        if (!cardPlay.Card.Keywords.Contains(CardKeyword.Exhaust)) return;

        int block = 8;
        if (cardPlay.Card is ZhongYanZhangBi source)
            block = (int)source.DynamicVars["BlockGain"].BaseValue;

        await CreatureCmd.GainBlock(base.Owner, block, ValueProp.Move, null, fast: true);
    }
}