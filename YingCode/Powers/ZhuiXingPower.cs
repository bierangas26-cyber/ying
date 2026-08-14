using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using System.Linq;
using System.Threading.Tasks;

namespace Yingmod.Ying.Powers;

[RegisterPower]
public sealed class ZhuiXingPower : IllusionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    // 由卡牌赋值的初始耐久（当前未在逻辑中使用，但保留以消除编译错误）
    public int InitialDurability { get; set; }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        // 1. 耐久 +2
        SetAmount(Amount + 2);
        UpdateDurabilityUI();

        // 2. 自动销毁：当前耐久 ≥ 10
        if (Amount >= 10)
        {
            int damage = Amount * 6;
            await Detonate(choiceContext, damage);
            Cleanup();
        }
    }

    // 被动破碎（被攻击或手动销毁）
    public override void OnBreak()
    {
        if (Owner?.Player == null) return;

        int damage = Amount * 4;
        Detonate(null, damage).Wait();
    }

    private async Task Detonate(PlayerChoiceContext? choiceContext, int totalDamage)
    {
        if (Owner?.Player == null || totalDamage <= 0) return;

        var enemies = Owner.CombatState.HittableEnemies.ToList();
        if (enemies.Count == 0) return;

        foreach (var enemy in enemies)
        {
            var creatureNode = NCombatRoom.Instance?.GetCreatureNode(enemy);
            if (creatureNode != null)
            {
                var vfx = NLargeMagicMissileVfx.Create(
                    creatureNode.GetBottomOfHitbox(),
                    new Color("50b598"));

                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
                if (vfx != null)
                    await Cmd.Wait(vfx.WaitTime);
            }
        }

        var context = choiceContext ?? new ThrowingPlayerChoiceContext();
        await CreatureCmd.Damage(context, enemies, totalDamage, ValueProp.Move, dealer: Owner);
    }

    private void Cleanup()
    {
        Manager?.RemoveIllusionUI(IllusionNode);
        Manager?.RemoveIllusionPower(this);
        _ = PowerCmd.Remove(this);
    }
}