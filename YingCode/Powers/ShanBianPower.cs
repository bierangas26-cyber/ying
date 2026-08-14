using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Cards;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class ShanBianPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 记录这张能力牌在打出时是否已经被升级
    public bool ProducesUpgradedMingYun { get; set; } = false;

    // 🌟 致敬《战鼓》官方源码：加入灾厄与消耗的悬浮提示
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<DoomPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(base.Owner))
        {
            // 1. 给予自身灾厄
            await PowerCmd.Apply<DoomPower>(new ThrowingPlayerChoiceContext(), base.Owner, base.Amount, base.Owner, null);

            // 2. 获取抽牌堆 (Draw Pile)
            var drawPile = PileType.Draw.GetPile(base.Owner.Player);
            if (drawPile != null && drawPile.Cards.Count > 0)
            {
                // 在引擎底层中，列表的最后一张牌 (Last) 也就是牌库的最顶端
                var topCard = drawPile.Cards.Last();

                // 对牌库顶的牌执行原生消耗指令
                await CardCmd.Exhaust(new ThrowingPlayerChoiceContext(), topCard);
            }

            // 3. 创建一张《命运》
            CardModel newMingYun = combatState.CreateCard<MingYun>(base.Owner.Player);

            // 如果你之前打出的是升级版的《嬗变》，生成的《命运》直接调用升级方法
            if (ProducesUpgradedMingYun)
            {
                CardCmd.Upgrade(newMingYun);
            }

            // 将《命运》直接印入手牌！
            await CardPileCmd.Add(newMingYun, PileType.Hand);
        }
    }
}