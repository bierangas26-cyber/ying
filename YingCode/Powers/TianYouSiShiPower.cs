using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Threading.Tasks;
using Yingmod.Ying.Patches;
using Yingmod.Ying.Powers;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class TianYouSiShiPower : IllusionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        int season = SeasonCyclePatch.GetCurrentSeason(player);
        if (season == 0) return;

        switch (season)
        {
            case 1: await ApplySpring(choiceContext); break;
            case 2: await ApplySummer(choiceContext); break;
            case 3: await ApplyAutumn(choiceContext); break;
            case 4: await ApplyWinter(choiceContext); break;
        }
    }

    private async Task ApplySpring(PlayerChoiceContext choiceContext)
    {
        await CreatureCmd.GainBlock(Owner, 6, ValueProp.Move, null);
        await PlayerCmd.GainEnergy(1, Owner.Player);

        // ★ 先加3点永久敏捷，再挂上回合结束时扣3点敏捷的 debuff，实现“临时敏捷”
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, 3, Owner, null);
        await PowerCmd.Apply<LinShiMinJiePower>(choiceContext, Owner, 3, Owner, null);
    }

    private async Task ApplySummer(PlayerChoiceContext choiceContext)
    {
        var enemies = Owner.CombatState.HittableEnemies;
        foreach (var enemy in enemies)
            await CreatureCmd.Damage(choiceContext, enemy, 7, ValueProp.Move, Owner, null);
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner, 5, Owner, null);
        await CardPileCmd.Draw(choiceContext, 1, Owner.Player);
    }

    private async Task ApplyAutumn(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<BlurPower>(choiceContext, Owner, 1, Owner, null);
    }

    private async Task ApplyWinter(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner, 4, Owner, null);
        await CreatureCmd.GainBlock(Owner, 7, ValueProp.Move, null);
    }
}