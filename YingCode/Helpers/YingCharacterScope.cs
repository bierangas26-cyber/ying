using System;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Yingmod.Ying.Helpers;

internal static class YingCharacterScope
{
    internal const string CharacterEntry = "YING_CHARACTER_YING";

    internal static bool IsYing(Player? player) =>
        player?.Character?.Id.Entry == CharacterEntry;

    internal static bool IsYingCard(CardModel? card) =>
        card?.GetType().Namespace?.StartsWith("Yingmod.Ying.", StringComparison.Ordinal) == true;
}
