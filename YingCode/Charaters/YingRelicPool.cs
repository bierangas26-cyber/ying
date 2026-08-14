using Godot;
using STS2RitsuLib.Scaffolding.Content; // 🌟 严格依据文档引入核心脚手架空间
using System;

namespace Yingmod.Ying.Character;

// 🌟 严格适配：对称变更为 RitsuLib 标准的 TypeListRelicPoolModel
public partial class YingRelicPool : TypeListRelicPoolModel
{
    public override string EnergyColorName => Ying.CharacterId;

    public override Color LabOutlineColor => Ying.Color;
}