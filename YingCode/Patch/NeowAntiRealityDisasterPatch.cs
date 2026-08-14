using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using Yingmod.Ying.Relics;

namespace Yingmod.Ying.Patch;

[HarmonyPatch]
public static class NeowAntiRealityDisasterPatch
{
    // 存储涅奥的原始选项和描述
    private static readonly Dictionary<AncientEventModel, List<EventOption>> _normalOptions = new();
    private static readonly Dictionary<AncientEventModel, LocString> _normalDescriptions = new();

    // 钩住受保护的方法 GenerateInitialOptionsWrapper
    [HarmonyPatch(typeof(AncientEventModel), "GenerateInitialOptionsWrapper")]
    [HarmonyPostfix]
    public static void ModifyInitialOptions(AncientEventModel __instance, ref IReadOnlyList<EventOption> __result)
    {
        // 仅对涅奥生效，不再限制角色
        if (__instance is not Neow neow || neow.Owner == null)
            return;

        if (HasStoredOriginalState(__instance))
            return;

        var originalOptions = __result.ToList();
        _normalOptions[__instance] = originalOptions;
        _normalDescriptions[__instance] = __instance.InitialDescription;

        __result = CreateCustomOptions(__instance);
    }

    private static IReadOnlyList<EventOption> CreateCustomOptions(AncientEventModel ancient)
    {
        var options = new List<EventOption>
        {
            // 选项①：获得反现实灾厄
            new EventOption(
                ancient,
                onChosen: async () =>
                {
                    await RelicCmd.Obtain<FanXianShiZaiE>(ancient.Owner!);
                    RestoreNormalOptionsOrFinish(ancient);
                },
                title: new LocString("relics", "YING_NEOW_GET_RELIC_TITLE"),
                description: new LocString("relics", "YING_NEOW_GET_RELIC_DESC"),
                textKey: "YING_NEOW_RELIC",
                Array.Empty<IHoverTip>()
            ).WithRelic<FanXianShiZaiE>(ancient.Owner!),

            // 选项②：跳过
            new EventOption(
                ancient,
                onChosen: () =>
                {
                    RestoreNormalOptionsOrFinish(ancient);
                    return Task.CompletedTask;
                },
                title: new LocString("relics", "YING_NEOW_SKIP_TITLE"),
                description: new LocString("relics", "YING_NEOW_SKIP_DESC"),
                textKey: "YING_NEOW_SKIP",
                Array.Empty<IHoverTip>()
            )
        };

        return options;
    }

    private static void RestoreNormalOptionsOrFinish(AncientEventModel ancient)
    {
        if (_normalOptions.TryGetValue(ancient, out var normalOpts) && normalOpts.Count > 0)
        {
            LocString description = _normalDescriptions.TryGetValue(ancient, out var desc) ? desc : ancient.InitialDescription;
            AccessTools.Method(typeof(EventModel), "SetEventState", new[] { typeof(LocString), typeof(IReadOnlyList<EventOption>) })
                ?.Invoke(ancient, new object[] { description, normalOpts });
            ClearStoredState(ancient);
            return;
        }

        LocString endDescription = ancient is Neow
            ? new LocString("events", "NEOW.pages.DONE.description")
            : ancient.InitialDescription;
        AccessTools.Method(typeof(EventModel), "SetEventFinished", new[] { typeof(LocString) })
            ?.Invoke(ancient, new object[] { endDescription });
        ClearStoredState(ancient);
    }

    private static bool HasStoredOriginalState(AncientEventModel ancient) =>
        _normalOptions.ContainsKey(ancient);

    private static void ClearStoredState(AncientEventModel ancient)
    {
        _normalOptions.Remove(ancient);
        _normalDescriptions.Remove(ancient);
    }
}