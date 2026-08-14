using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Singletons;

[RegisterSingleton]
public class AntiRealityDisasterSingleton : HookedSingletonModel
{
    public static AntiRealityDisasterSingleton? Instance { get; private set; }

    public AntiRealityDisasterSingleton() : base(HookType.Combat)
    {
        Instance = this;
    }

    private readonly HashSet<ICombatState> _processedRounds = new();
    // 缓存最终评级等级（1~7），不依赖怪物实例
    private int _finalRatingLevel = 1;

    public int GetFinalRatingLevel() => _finalRatingLevel;

    public override Task BeforeCombatStart()
    {
        _processedRounds.Clear();
        _finalRatingLevel = 1;
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null || _processedRounds.Contains(combatState)) return;
        _processedRounds.Add(combatState);

        var monsters = combatState.Enemies.Where(e => e.IsAlive).ToList();
        if (!monsters.Any()) return;

        foreach (var monster in monsters)
        {
            var ratings = monster.GetPowerInstances<BaseRatingPower>().ToList();
            if (!ratings.Any()) continue;

            int finalLevel;

            if (ratings.Count == 1)
            {
                // 单人模式或只有一个评级：直接使用该评级
                finalLevel = ratings[0].RatingLevel;
                // 无需移除再添加，直接保留原能力
            }
            else
            {
                // 多人模式：取平均值
                finalLevel = combatState.RoundNumber == 1
                    ? ratings.Min(r => r.RatingLevel)
                    : (int)ratings.Average(r => r.RatingLevel);

                // 清除旧评级
                foreach (var rating in ratings)
                    await PowerCmd.Remove(rating);

                // 应用新评级
                await ApplyRating(choiceContext, monster, finalLevel);
            }

            // 更新全局缓存
            _finalRatingLevel = finalLevel;
        }
    }

    private static async Task ApplyRating(PlayerChoiceContext context, Creature monster, int level)
    {
        switch (level)
        {
            case 1: await PowerCmd.Apply<RatingD>(context, monster, 1, null, null); break;
            case 2: await PowerCmd.Apply<RatingC>(context, monster, 1, null, null); break;
            case 3: await PowerCmd.Apply<RatingB>(context, monster, 1, null, null); break;
            case 4: await PowerCmd.Apply<RatingA>(context, monster, 1, null, null); break;
            case 5: await PowerCmd.Apply<RatingS>(context, monster, 1, null, null); break;
            case 6: await PowerCmd.Apply<RatingSS>(context, monster, 1, null, null); break;
            case 7: await PowerCmd.Apply<RatingSSS>(context, monster, 1, null, null); break;
        }
    }
}