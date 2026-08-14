using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class PianXianTaXingGuangPower : PowerModel, ISecondaryResourceHookListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>()
    ];

    // ==========================================
    // 监听心脏充能（次要资源）的变化事件
    // ==========================================
    public async Task AfterSecondaryResourceChanged(SecondaryResourceChangeContext context)
    {
        // 只处理“心脏充能”资源，且必须是获得（增加）事件
        if (context.Definition.Id != MainFile.RellyId || context.Delta <= 0)
            return;

        // 确保能力拥有者匹配当前变化的玩家
        if (context.Player?.Creature != base.Owner)
            return;

        // 1. 获得活力（层数等于此能力的层数）
        await PowerCmd.Apply<VigorPower>(null, base.Owner, base.Amount, base.Owner, null);

        // 2. 抽2张牌
        if (base.Owner.Player != null)
        {
            await CardPileCmd.Draw(null!, 2m, base.Owner.Player);
        }
    }
}