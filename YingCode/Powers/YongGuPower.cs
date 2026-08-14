using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;
using Yingmod.Ying; // MainFile.RellyId

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class YongGuPower : PowerModel, ISecondaryResourceHookListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // ==========================================
    // 监听心脏充能变化，当充能增加时获得格挡
    // ==========================================
    public async Task AfterSecondaryResourceChanged(SecondaryResourceChangeContext context)
    {
        // 1. 必须是心脏充能资源
        if (context.Definition.Id != MainFile.RellyId)
            return;

        // 2. 必须是增加（获得）事件，消耗充能时不触发
        if (context.Delta <= 0)
            return;

        // 3. 确保能力拥有者匹配
        if (context.Player?.Creature != base.Owner)
            return;

        // 获得格挡（格挡值等于此能力的层数）
        await CreatureCmd.GainBlock(base.Owner, (decimal)base.Amount, (ValueProp)0, null);
    }
}