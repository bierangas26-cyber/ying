using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Runs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying;
using Yingmod.Ying.Keywords;

namespace Yingmod.Ying.Patches;

[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
public static class ResetSeasonPatch
{
    public static void Postfix(IRunState runState, ICombatState? combatState)
    {
        SeasonCyclePatch.ResetCounter();
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterPlayerTurnStart))]
public static class SeasonCyclePatch
{
    private sealed class SeasonState
    {
        public int CurrentSeason;
        public int LastTurnNumber = -1;
    }

    private static readonly Dictionary<ulong, SeasonState> _states = new();

    public static int GetCurrentSeason(Player player) =>
        _states.TryGetValue(player.NetId, out var state) ? state.CurrentSeason : 0;

    public static void ResetCounter()
    {
        _states.Clear();
    }

    // 恢复为标准的 async void Postfix，直接调用实际逻辑
    public static async void Postfix(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Character.Id.Entry != "YING_CHARACTER_YING") return;

        var state = GetOrCreateState(player);
        int turnNumber = player.PlayerCombatState?.TurnNumber ?? 0;
        if (turnNumber == state.LastTurnNumber) return;

        state.LastTurnNumber = turnNumber;
        state.CurrentSeason = state.CurrentSeason % 4 + 1;

        if (state.CurrentSeason == 1)
        {
            var pile = MainFile.RebirthPile.GetPile(player);
            if (pile != null)
            {
                foreach (var card in pile.Cards.ToList())
                {
                    if ((card.Type == CardType.Attack || card.Type == CardType.Skill)
                        && !card.Keywords.Contains(YingKeywords.LunHui))
                    {
                        CardCmd.ApplyKeyword(card, YingKeywords.LunHui);
                    }
                }

                for (int i = 0; i < 2; i++)
                {
                    if (pile.Cards.Count == 0) break;
                    var topCard = pile.Cards.First();
                    await CardPileCmd.Add(topCard, PileType.Hand);
                }
            }
        }

        SeasonCalendarUiPatch.UpdateSeason(player, state.CurrentSeason);
    }

    private static SeasonState GetOrCreateState(Player player)
    {
        if (!_states.TryGetValue(player.NetId, out var state))
        {
            state = new SeasonState();
            _states[player.NetId] = state;
        }
        return state;
    }
}