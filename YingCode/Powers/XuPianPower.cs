using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId
using Yingmod.Ying.Cards;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class XuPianPower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != base.Owner) return;
        if (!cardPlay.Card.Keywords.Contains(CardKeyword.Exhaust)) return;

        int draw = 1;
        int gain = 2;
        if (cardPlay.Card is XuPian source)
        {
            draw = (int)source.DynamicVars["DrawCount"].BaseValue;
            gain = (int)source.DynamicVars["RellyGain"].BaseValue;
        }

        if (draw > 0)
            await CardPileCmd.Draw(choiceContext, draw, base.Owner.Player);
        if (gain > 0)
            // 次要资源系统：获得心脏充能
            await SecondaryResourceCmd.Gain(base.Owner.Player, MainFile.RellyId, gain);
    }
}