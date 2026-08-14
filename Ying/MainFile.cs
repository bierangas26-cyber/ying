using System.Reflection;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Combat.SecondaryResources;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.CardPiles;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using Yingmod.Ying.Cards;
using Yingmod.Ying.Relics;
using Yingmod.Ying.Helpers;   // ★ 新增：引入角色判断辅助类

namespace Yingmod.Ying;

[ModInitializer(nameof(Initialize))]
public class MainFile
{
    public const string ModId = "Ying";
    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; private set; } = null!;
    public static PileType RebirthPile;
    public static SecondaryResourceDefinition RellyDef { get; private set; } = null!;
    public static string RellyId => RellyDef.Id;

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();
        Logger = RitsuLibFramework.CreateLogger(ModId);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);

        // ────── 注册轮回牌堆 ──────
        var pileRegistry = ModCardPileRegistry.For(ModId);
        RebirthPile = pileRegistry.RegisterOwned("rebirth_pile", new ModCardPileSpec
        {
            Scope = ModCardPileScope.CombatOnly,
            Style = ModCardPileUiStyle.BottomLeft,
            Anchor = ModCardPileAnchor.Default,
            IconPath = "res://Ying/Images/UI/rebirth_pile.png",
            OnOpen = ctx => ctx.ShowDefaultPileScreen(),
            // ★ 修改：仅对 Ying 角色显示轮回堆
            VisibleWhen = ctx => YingCharacterScope.IsYing(ctx.Player)
        }).PileType;

        // ────── 注册心脏充能作为次要资源 ──────
        var resReg = RitsuLibFramework.GetSecondaryResourceRegistry(ModId);
        RellyDef = resReg.Register("heart_charge", new SecondaryResourceDefinition(
            defaultAmount: 0,
            baseMaxAmount: null,                              // 无上限
            turnStartPolicy: SecondaryResourceTurnStartPolicy.None,  // 不自动变化
            persistencePolicy: SecondaryResourcePersistencePolicy.Combat,
            smallIconPath: "res://Ying/Images/UI/relly_small.png",
            largeIconPath: "res://Ying/Images/UI/relly_large.png"
        ));

        // 注意：不再使用 RitsuLib 内置计数器，改为 HeartbeatNodePatch + 桥接能力

        // ────── 先古/遗物升级映射 ──────
        RitsuLibFramework.RegisterArchaicToothTranscendenceMapping<YingPowerUp, GongNengZhongDuan>();
        RitsuLibFramework.RegisterTouchOfOrobasRefinementMapping<YingHeart, BlueHeart>();

        // ────── Harmony 补丁 ──────
        Harmony harmony = new(ModId);
        harmony.PatchAll();
    }
}