#nullable enable // 🌟 终极修复：开启可空上下文，直接消灭满屏的“只能在 #nullable 注释上下文内使用...”风格警告！

using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib;
using STS2RitsuLib.Data.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Godot;
using System;
using System.Collections.Generic;
using Yingmod.Ying.Cards;
using Yingmod.Ying.Extensions;
using Yingmod.Ying.Relics;

namespace Yingmod.Ying.Character;

[RegisterCharacter]
public class Ying : ModCharacterTemplate<YingCardPool, YingRelicPool, SharedPotionPool>
{
	public const string CharacterId = "Ying";

	public static readonly Color Color = new Color("c4278a");

	public override Color NameColor => Color;
	public override CharacterGender Gender => CharacterGender.Feminine;

	// 初始血量与金币配置
	public override int StartingHp => 70;
	public override int StartingGold => 99;

	public override float AttackAnimDelay => 0.3f;
	public override float CastAnimDelay => 0.1f;

	// ★ 跳过 Epoch 和时间线需求，修复战斗胜利后卡死
	public override bool RequiresEpochAndTimeline => false;

	public override CharacterAssetProfile AssetProfile => CharacterAssetProfiles.Merge(
		CharacterAssetProfiles.Ironclad(),
		new(
			Scenes: new(
				VisualsPath: "res://Ying/Scenes/ying.tscn",
				// 🌟 新增：注册自定义能量表盘的场景路径
				EnergyCounterPath: "res://Ying/Scenes/ying_energy_counter.tscn",

				MerchantAnimPath: "res://Ying/Scenes/ying_character_merchant.tscn.tscn",

				RestSiteAnimPath: "res://Ying/Scenes/ying_character_rest_site.tscn"
			),
			Ui: new(
				IconTexturePath: "character_icon_char_name.png".CharacterUiPath(),
				CharacterSelectBgPath: "res://Ying/Scenes/char_select_bg_ying.tscn",
				CharacterSelectIconPath: "char_select_char_name.png".CharacterUiPath(),
				CharacterSelectLockedIconPath: "char_select_char_name_locked.png".CharacterUiPath(),
				MapMarkerPath: "map_marker_char_name.png".CharacterUiPath()
			),

			Audio: new(
				// 攻击音效：借用铁甲战士的重击音效
				AttackSfx: "event:/sfx/char/ironclad/attack_heavy",
				// 施法音效：借用静默猎手的施法音效
				CastSfx: "event:/sfx/char/silent/cast",
				// 死亡音效
				DeathSfx: "event:/sfx/char/ironclad/death",
				// 角色选择界面点击时的音效
				CharacterSelectSfx: "event:/sfx/ui/char_select_ironclad",
				// 过渡音效
				CharacterTransitionSfx: "event:/sfx/ui/wipe_ironclad"
			)
		)
	);

	protected override NCreatureVisuals? TryCreateCreatureVisuals() =>
		RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.Scenes!.VisualsPath!);

	// 🌟 终极修复：显式挂载 [Obsolete] 特性，严格听从编译器指挥，消除该条黄点警告！
	[Obsolete]
	protected override IEnumerable<StartingDeckEntry> StartingDeckEntries => [
		new(typeof(YingAttack), 4),
		new(typeof(YingBlock), 4),
		new(typeof(ZhuShiWaLiShi), 1),
		new(typeof(YingPowerUp), 1),
		new(typeof(XinYuQiDao), 1),
		new(typeof(JinJiGongNeng), 1),
		new(typeof(ChongNengDaJi), 1),
		new(typeof(LunHui), 1)
	];

	// 🌟 终极修复：显式挂载 [Obsolete] 特性，严格听从编译器指挥，消除该条黄点警告！
	[Obsolete]
	protected override IEnumerable<Type> StartingRelicTypes => [
		typeof(YingHeart)
	];

	public override List<string> GetArchitectAttackVfx() => [
		 "vfx/vfx_attack_blunt",
		"vfx/vfx_heavy_blunt",
		"vfx/vfx_attack_slash",
		"vfx/vfx_bloody_impact",
        "vfx/vfx_rock_shatter"
	];
}
