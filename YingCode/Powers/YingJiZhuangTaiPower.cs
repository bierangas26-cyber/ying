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
public sealed class YingJiZhuangTaiPower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;   // 不叠加，每次独立生效

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != base.Owner) return;
        Flash();

        // 根据自身层数生成对应数量的紧急供能（基础2层，升级后3层）
        for (int i = 0; i < (int)base.Amount; i++)
        {
            var card = player.Creature.CombatState.CreateCard<JinJiGongNeng>(player);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
        }
    }
}