using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class BeiMingWhalePower : IllusionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    // 致命破碎：免疫死亡，回血、降上限、加牌（全部延迟）
    public void OnFatalBreak()
    {
        if (Owner?.Player == null) return;
        // 挂上致命破碎专用能力
        _ = PowerCmd.Apply<BeiMingWhaleFatalBreakPower>(null, Owner, 1, Owner, null);
        Break(); // 移除自身UI和能力
    }

    // 普通破碎（非致命）：只加牌
    public override void OnBreak()
    {
        if (Owner?.Player == null) return;
        // 挂上普通破碎专用能力
        _ = PowerCmd.Apply<BeiMingWhaleNormalBreakPower>(null, Owner, 1, Owner, null);
    }
}