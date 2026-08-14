using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using System.Linq;
using Yingmod.Ying.UI;

namespace Yingmod.Ying.Patches;

[HarmonyPatch(typeof(NCombatUi), "Activate")]
public static class SeasonCalendarUiPatch
{
    public static void Postfix(NCombatUi __instance, CombatState state)
    {
        var localPlayer = state.Players.OfType<Player>().FirstOrDefault(LocalContext.IsMe);
        if (localPlayer == null || localPlayer.Character.Id.Entry != "YING_CHARACTER_YING")
            return;

        if (__instance.GetNodeOrNull<Control>("SeasonCalendar") == null)
        {
            var scene = GD.Load<PackedScene>("res://Ying/Scenes/SeasonCalendarUI.tscn");
            if (scene != null)
            {
                var calendar = scene.Instantiate<SeasonCalendarUI>();
                calendar.Name = "SeasonCalendar";
                __instance.AddChild(calendar);
                calendar.Position = new Vector2(1600, 20);
            }
        }
    }

    internal static void UpdateSeason(Player player, int season)
    {
        if (!LocalContext.IsMe(player) || player.Character.Id.Entry != "YING_CHARACTER_YING")
            return;

        var combatRoom = NCombatRoom.Instance;
        if (combatRoom == null) return;
        var combatUi = combatRoom.Ui;
        if (combatUi == null) return;

        var calendar = combatUi.GetNodeOrNull<SeasonCalendarUI>("SeasonCalendar");
        calendar?.UpdateSeason(season);
    }
}