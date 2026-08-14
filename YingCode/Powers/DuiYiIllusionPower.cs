using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;
using Yingmod.Ying.Cards;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class DuiYiIllusionPower : IllusionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        var rng = player.RunState.Rng.CombatCardGeneration;
        var combatState = player.Creature.CombatState;

        for (int i = 0; i < 2; i++)
        {
            int index = rng.NextInt(0, 4);
            CardModel? card = null;

            switch (index)
            {
                case 0:
                    card = combatState.CreateCard<YiAttack>(player);
                    break;
                case 1:
                    card = combatState.CreateCard<YiDefend>(player);
                    break;
                case 2:
                    card = combatState.CreateCard<YiBuff>(player);
                    break;
                case 3:
                    card = combatState.CreateCard<YiDebuff>(player);
                    break;
            }

            if (card != null)
                await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
        }
    }
}