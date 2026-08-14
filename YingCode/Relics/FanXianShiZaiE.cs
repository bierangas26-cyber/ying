using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yingmod.Ying.Character;
using Yingmod.Ying.Powers;
using Yingmod.Ying.Singletons;

namespace Yingmod.Ying.Relics;

[RegisterRelic(typeof(YingRelicPool))]
public class FanXianShiZaiE : YingRelics
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("BlockTurns", 3m),     // 获得格挡的回合数
        new HealVar(8m)                      // 战后回血百分比 (参考 BurningBlood)
    ];

    // ==================== 原有战斗评级逻辑 ====================
    public override async Task BeforeCombatStart()
    {
        var combatState = Owner?.Creature?.CombatState;
        if (combatState == null) return;

        var monsters = combatState.Enemies.Where(e => e.IsAlive).ToList();
        if (monsters.Count == 0) return;

        MegaCrit.Sts2.Core.Logging.Log.Info($"[反现实灾厄] 战斗开始，怪物数量: {monsters.Count}");

        var rng = combatState.RunState.Rng.CombatCardGeneration;
        int idx = rng.NextInt(0, 7);

        foreach (var monster in monsters)
        {
            switch (idx)
            {
                case 0: await PowerCmd.Apply<RatingD>(new ThrowingPlayerChoiceContext(), monster, 1, null, null); break;
                case 1: await PowerCmd.Apply<RatingC>(new ThrowingPlayerChoiceContext(), monster, 1, null, null); break;
                case 2: await PowerCmd.Apply<RatingB>(new ThrowingPlayerChoiceContext(), monster, 1, null, null); break;
                case 3: await PowerCmd.Apply<RatingA>(new ThrowingPlayerChoiceContext(), monster, 1, null, null); break;
                case 4: await PowerCmd.Apply<RatingS>(new ThrowingPlayerChoiceContext(), monster, 1, null, null); break;
                case 5: await PowerCmd.Apply<RatingSS>(new ThrowingPlayerChoiceContext(), monster, 1, null, null); break;
                case 6: await PowerCmd.Apply<RatingSSS>(new ThrowingPlayerChoiceContext(), monster, 1, null, null); break;
            }
        }
    }

    // ==================== 新增：前 3 回合格挡 (参考 RingOfTheDrake) ====================
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;

        int currentTurn = Owner.PlayerCombatState?.TurnNumber ?? 0;
        if (currentTurn <= (int)DynamicVars["BlockTurns"].BaseValue)
        {
            Flash();
            await CreatureCmd.GainBlock(Owner.Creature, 12, ValueProp.Move, null);
        }
    }

    // ==================== 新增：战后回血 (参考 BurningBlood) ====================
    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (!Owner.Creature.IsDead)
        {
            Flash();
            int healAmount = (int)(Owner.Creature.MaxHp * (DynamicVars.Heal.BaseValue / 100m));
            await CreatureCmd.Heal(Owner.Creature, healAmount);
        }
    }

    // ==================== 原有战斗奖励金币逻辑 ====================
    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (player != Owner || room == null || !room.RoomType.IsCombatRoom())
            return false;
        if (room.RoomType == RoomType.Boss && Owner.RunState.CurrentActIndex >= Owner.RunState.Acts.Count - 1)
            return false;

        int ratingLevel = 1;
        var singleton = AntiRealityDisasterSingleton.Instance;
        if (singleton != null)
            ratingLevel = singleton.GetFinalRatingLevel();

        int goldAmount = ratingLevel switch
        {
            1 => 20,
            2 => 30,
            3 => 50,
            4 => 60,
            5 => 100,
            6 => 150,
            7 => 200,
            _ => 0
        };

        if (goldAmount > 0)
        {
            rewards.Add(new GoldReward(goldAmount, Owner));
            Flash();
            return true;
        }
        return false;
    }
}