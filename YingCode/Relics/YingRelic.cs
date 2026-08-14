using Godot;
using STS2RitsuLib;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Yingmod.Ying.Character;
using Yingmod.Ying.Extensions;

namespace Yingmod.Ying.Relics;

// 🌟 核心修复：移除了原本错挂在抽象遗物基类头上的 [RegisterRelic] 特性。
public abstract class YingRelics : ModRelicTemplate
{
    // 🌟 一劳永逸的修复：直接删掉注册前缀，保留完整的 ying_heart 名称
    private string GetCleanName()
    {
        if (string.IsNullOrEmpty(Id.Entry)) return "relic";

        // 直接把框架要求的标准前缀抹掉，转为小写
        return Id.Entry.Replace("YING_RELIC_", "").ToLowerInvariant();
    }

    // 🌟 完美保留你重写的小图标路径逻辑
    public override string CustomIconPath
    {
        get
        {
            var path = $"{GetCleanName()}.png".RelicImagePath();
            return ResourceLoader.Exists(path) ? path : "relic.png".RelicImagePath();
        }
    }

    // 🌟 完美保留你重写的图标描边路径逻辑
    public override string CustomIconOutlinePath
    {
        get
        {
            var path = $"{GetCleanName()}_outline.png".RelicImagePath();
            return ResourceLoader.Exists(path) ? path : "relic_outline.png".RelicImagePath();
        }
    }

    // 🌟 完美保留你重写的大图标路径逻辑
    public override string CustomBigIconPath
    {
        get
        {
            var path = $"{GetCleanName()}.png".BigRelicImagePath();
            return ResourceLoader.Exists(path) ? path : "relic.png".BigRelicImagePath();
        }
    }
}