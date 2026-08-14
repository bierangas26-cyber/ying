using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;
using Yingmod.Ying.Keywords;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class KongJianMiShiPower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != base.Owner)
            return;

        if (cardPlay.Card.Keywords.Contains(YingKeywords.JingZhi))
        {
            foreach (Creature enemy in base.Owner.CombatState.HittableEnemies)
            {
                await PowerCmd.Apply<ShiJianZhiHuanPower>(
                    choiceContext,
                    enemy,
                    8m,
                    base.Owner,
                    cardPlay.Card
                );
            }
        }
    }
}