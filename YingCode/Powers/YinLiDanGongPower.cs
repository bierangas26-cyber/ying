using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Linq;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class YinLiDanGongPower : YingPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public CardModel? CreatorCard { get; set; }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var playedCard = cardPlay.Card;
        if (playedCard?.Owner?.Creature != Owner) return;
        if (playedCard.Type != CardType.Attack) return;

        // 跳过产生此能力的引力弹弓本身
        if (CreatorCard != null && playedCard == CreatorCard)
        {
            CreatorCard = null;
            return;
        }

        // 原有单目标逻辑
        if (cardPlay.Target != null)
        {
            await PowerCmd.Apply<ShiJianZhiHuanPower>(choiceContext, cardPlay.Target, (int)Amount, Owner, null);
        }
        else
        {
            // 群攻牌没有单一目标，对所有可攻击敌人施加
            var enemies = Owner.CombatState.HittableEnemies;
            foreach (var enemy in enemies)
            {
                await PowerCmd.Apply<ShiJianZhiHuanPower>(choiceContext, enemy, (int)Amount, Owner, null);
            }
        }

        // 一次性效果
        await PowerCmd.Remove(this);
    }
}