using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2RitsuLib.Combat.SecondaryResources;
using Yingmod.Ying;

namespace Yingmod.Ying.Patch;

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
public static class CardHeartbeatCostPatch
{
    public static void Postfix(NCard __instance)
    {
        CardModel cardModel = __instance.Model;
        if (cardModel == null) return;

        // 1. 读取费用动态变量
        DynamicVar? costVar = null;
        if (cardModel.DynamicVars.TryGetValue("RellyCost", out var v)) costVar = v;
        else if (cardModel.DynamicVars.TryGetValue("RellyPower", out v)) costVar = v;
        else if (cardModel.DynamicVars.TryGetValue("CostAmount", out v)) costVar = v;

        // 2. 无消耗 → 隐藏标签
        if (costVar == null)
        {
            var existing = __instance.Body?.GetNodeOrNull<Control>("CardHeartbeatCost")
                        ?? __instance.GetNodeOrNull<Control>("CardHeartbeatCost");
            if (existing != null) existing.Visible = false;
            return;
        }

        // 3. 获取或创建标签节点
        var costNode = __instance.Body?.GetNodeOrNull<Control>("CardHeartbeatCost")
                    ?? __instance.GetNodeOrNull<Control>("CardHeartbeatCost");

        if (costNode == null)
        {
            var prefab = GD.Load<PackedScene>("res://Ying/Scenes/CardHeartbeatCost.tscn");
            if (prefab == null) return;

            costNode = prefab.Instantiate<Control>();
            costNode.Name = "CardHeartbeatCost";

            if (__instance.Body != null)
                __instance.Body.AddChild(costNode);
            else
                __instance.AddChild(costNode);
        }

        // 4. 显示费用（使用 BaseValue 确保任何环境都读到正确数值）
        decimal cost = System.Math.Abs(costVar.BaseValue);
        costNode.Visible = true;

        var richLabel = costNode.GetNodeOrNull<RichTextLabel>("CostLabel");
        if (richLabel != null)
        {
            richLabel.Text = cost.ToString();

            // 5. 核心修复：只有在非图鉴（可变实例）时，才去读取 Owner 以判定颜色
            if (!cardModel.IsCanonical)
            {
                int currentCharge = 0;
                try
                {
                    // 这里是安全的，只有可变实例才会执行到这里
                    if (cardModel.Owner != null)
                        currentCharge = SecondaryResourceCmd.Get(cardModel.Owner, MainFile.RellyId);
                }
                catch
                {
                    // 兜底保护，currentCharge 保持为 0
                }

                Color textColor = currentCharge >= cost ? new Color(1, 1, 1) : new Color(1f, 0.4f, 0.6f);
                richLabel.AddThemeColorOverride("default_color", textColor);
            }
            else
            {
                // 图鉴中的卡，统一显示为白色
                richLabel.AddThemeColorOverride("default_color", new Color(1, 1, 1));
            }
        }
    }
}