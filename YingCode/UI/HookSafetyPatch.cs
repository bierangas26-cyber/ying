using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace Yingmod.Ying.Patch;

[HarmonyPatch(typeof(Hook), "IterateCombatHookListeners")]
public static class HookSafetyPatch
{
    public static IEnumerable<AbstractModel> Postfix(IEnumerable<AbstractModel> __result)
    {
        if (__result == null)
            yield break;

        var enumerator = __result.GetEnumerator();
        while (true)
        {
            AbstractModel current;
            try
            {
                if (!enumerator.MoveNext())
                    break; // 正常结束
                current = enumerator.Current;
            }
            catch (Exception ex)
            {
                // 捕获迭代内部异常，跳过，避免崩溃
                Log.Error($"[HookSafety] Caught exception during Hook iteration: {ex.Message}");
                yield break; // 安全终止
            }

            // 跳过 null 元素和 Owner 为 null 的能力
            if (current == null)
            {
                Log.Error("[HookSafety] Skipped null model element.");
                continue;
            }

            if (current is PowerModel power && power.Owner == null)
            {
                Log.Error($"[HookSafety] Skipped power with null Owner: {power.Id}");
                continue;
            }

            yield return current;
        }
    }
}