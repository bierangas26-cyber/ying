using Godot;
using STS2RitsuLib.Scaffolding.Content; // 🌟 严格依据文档引入核心空间
using STS2RitsuLib.Utils;               // 🌟 引入用于卡框材质的工具包
using Yingmod.Ying.Extensions;

namespace Yingmod.Ying.Character;

// 🌟 严格适配：由 CustomCardPoolModel 变更为 RitsuLib 规范的 TypeListCardPoolModel
public class YingCardPool : TypeListCardPoolModel
{
    // 卡池的 ID。必须唯一防撞车。
    public override string Title => Ying.CharacterId;

    // 能量颜色名称，通常与角色 ID 保持一致
    public override string EnergyColorName => Ying.CharacterId;

    // 描述中使用的能量图标。大小为 24x24。
    public override string? TextEnergyIconPath => "Charui/text_energy.png".ImagePath();

    // 结算和卡牌左上角的能量图标。大小为 74x74。
    public override string? BigEnergyIconPath => "Charui/big_energy.png".ImagePath();

    // 卡池的主题色（小卡牌图标颜色）
    public override Color DeckEntryCardColor => new("840240");

    // 能量圈文字轮廓颜色
    public override Color EnergyOutlineColor => new("651565");

    // 🌟 严格依据文档：使用 RgbShaderMaterial 材质完美替换以前过时的 H/S/V 浮点数控制
    // 这里传入的参数为标准 RGB 的比例值（对应你的卡池主题色）
    // 🌟 完美对齐新版框架：换成官方推荐的全新 Hue 材质方法
    // 🌟 严格依据教程：使用自定义卡框专用的 UnmodulatedHsvShaderMaterial，防止系统给你的原画乱上色
    private static readonly Material? _poolFrameMaterial = MaterialUtils.CreateUnmodulatedHsvShaderMaterial();
    public override Material? PoolFrameMaterial => _poolFrameMaterial;

    // 卡池是否为无色卡池
    public override bool IsColorless => false;

}