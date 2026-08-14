using Godot;
using STS2RitsuLib;
using STS2RitsuLib.Scaffolding.Content;
using Yingmod.Ying.Extensions;

namespace Yingmod.Ying.Powers;

public abstract class YingPower : ModPowerTemplate
{
    // 修正：移除前缀 "YING_POWER_"，并转为小写，与遗物基类逻辑一致
    private string GetCleanName()
    {
        if (string.IsNullOrEmpty(Id.Entry)) return "power";
        return Id.Entry.Replace("YING_POWER_", "").ToLowerInvariant();
    }

    public override string CustomIconPath
    {
        get
        {
            var path = $"{GetCleanName()}.png".PowerImagePath();    // 使用现有的扩展方法
            return ResourceLoader.Exists(path) ? path : "power.png".PowerImagePath();
        }
    }

    public override string CustomBigIconPath
    {
        get
        {
            var path = $"{GetCleanName()}.png".BigPowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".BigPowerImagePath();
        }
    }
}