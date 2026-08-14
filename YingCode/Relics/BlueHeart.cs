using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;
using Yingmod.Ying.Character;

namespace Yingmod.Ying.Relics;

[RegisterRelic(typeof(YingRelicPool))]
public class BlueHeart : YingRelics
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;

        if (player.Creature.CombatState?.RoundNumber == 1)
        {
            await SecondaryResourceCmd.Gain(player, MainFile.RellyId, 10);
        }
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner.Creature || result.UnblockedDamage <= 0) return;

        await SecondaryResourceCmd.Gain(Owner, MainFile.RellyId, 4);
        await PowerCmd.Remove<BlockNextTurnPower>(Owner.Creature);
        await PowerCmd.Apply<BlockNextTurnPower>(choiceContext,
            Owner.Creature, 6m, applier: Owner.Creature, cardSource: null);
    }

    public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner.Creature) return amount;

        decimal hpPercent = (decimal)target.CurrentHp / target.MaxHp;
        if (hpPercent <= 0.3m)
        {
            decimal cap = target.MaxHp * 0.1m;
            return amount > cap ? cap : amount;
        }
        return amount;
    }

    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (player != base.Owner) return false;
        if (room == null || room.RoomType != RoomType.Monster) return false;

        rewards.Add(new CardReward(CardCreationOptions.ForRoom(player, RoomType.Monster), 3, player));
        return true;
    }
}