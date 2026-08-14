using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Logging;
using STS2RitsuLib;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using System.Collections.Generic;
using System.Linq;
using Yingmod.Ying.Character;
using Yingmod.Ying.Extensions;
using Yingmod.Ying.Keywords;

namespace Yingmod.Ying.Cards;

// 抽象模板类，不需要注册
public abstract class YingCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    ModCardTemplate(cost, type, rarity, target)
{
    protected string GetCleanName()
    {
        if (string.IsNullOrEmpty(Id.Entry)) return "card";
        string entry = Id.Entry;
        entry = entry.Replace("YINGMOD-", "").Replace("YING_CARD_", "");
        return entry.ToLowerInvariant();
    }

    public override CardAssetProfile AssetProfile
    {
        get
        {
            var path = $"{GetCleanName()}.png".CardImagePath();
            Log.Info(">>>[YingMod]CardPath=" + path, 2);
            var finalPortraitPath = ResourceLoader.Exists(path) ? path : "card.png".CardImagePath();

            string framePath = "";
            string bannerPath = "";
            string borderPath = "";

            switch (this.Type)
            {
                case CardType.Attack:
                    framePath = "res://Ying/Images/Frames/attack_frame.png";
                    bannerPath = "res://Ying/Images/Frames/attack_banner.png";
                    borderPath = "res://Ying/Images/Frames/attack_border.png";
                    break;
                case CardType.Skill:
                    framePath = "res://Ying/Images/Frames/skill_frame.png";
                    bannerPath = "res://Ying/Images/Frames/skill_banner.png";
                    borderPath = "res://Ying/Images/Frames/skill_border.png";
                    break;
                case CardType.Power:
                    framePath = "res://Ying/Images/Frames/power_frame.png";
                    bannerPath = "res://Ying/Images/Frames/power_banner.png";
                    borderPath = "res://Ying/Images/Frames/power_border.png";
                    break;
            }

            return new CardAssetProfile(
                PortraitPath: finalPortraitPath,
                FramePath: framePath,
                BannerTexturePath: bannerPath,
                PortraitBorderPath: borderPath
            );
        }
    }

    // 不需要重写 AdditionalHoverTips，官方会自动为所有关键词生成提示。
}