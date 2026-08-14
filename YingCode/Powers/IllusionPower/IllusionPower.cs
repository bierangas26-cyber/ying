using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yingmod.Ying.Capabilities;

namespace Yingmod.Ying.Powers;

public abstract class IllusionPower : YingPower
{
    public Control? IllusionNode { get; set; }
    public IllusionManagerCapability? Manager { get; set; }
    public Label? TooltipLabel { get; set; }

    private bool _isReducing = false;

    // 静态防重复扣除机制：记录每个幻境实例最后一次扣除的时间和数量
    private static readonly Dictionary<string, (double time, int amount)> _lastReduce = new();
    private static readonly object _lock = new();

    public void ReduceDurability(int amount)
    {
        if (_isReducing) return;

        // 构造唯一标识：玩家NetId + 幻境实例的哈希码
        string key = $"{Owner?.Player?.NetId}_{GetHashCode()}";
        double now = Time.GetTicksMsec() / 1000.0; // 秒

        lock (_lock)
        {
            // 如果 50ms 内同一玩家同一幻境收到了相同的扣除量，视为重复调用，直接忽略
            if (_lastReduce.TryGetValue(key, out var last) &&
                Math.Abs(now - last.time) < 0.05 &&
                last.amount == amount)
            {
                return;
            }
            _lastReduce[key] = (now, amount);
        }

        _isReducing = true;
        try
        {
            SetAmount(Amount - amount);
            UpdateDurabilityUI();
        }
        finally
        {
            _isReducing = false;
        }
    }

    public void SetDurability(int newAmount)
    {
        SetAmount(newAmount);
        UpdateDurabilityUI();
    }

    public virtual void OnBreak() { }

    public void Break()
    {
        OnBreak();
        if (IllusionNode != null && GodotObject.IsInstanceValid(IllusionNode))
        {
            IllusionNode.QueueFree();
            IllusionNode = null;
        }
        Manager?.RemoveIllusionPower(this);
        _ = PowerCmd.Remove(this);
    }

    public void UpdateDurabilityUI()
    {
        if (IllusionNode == null || !GodotObject.IsInstanceValid(IllusionNode)) return;
        var label = IllusionNode.GetNodeOrNull<Label>("DurabilityLabel");
        if (label != null) label.Text = Amount.ToString();
    }

    public void UpdateTooltipLabel()
    {
        if (TooltipLabel == null || !GodotObject.IsInstanceValid(TooltipLabel)) return;
        string baseDesc = Description.GetFormattedText();
        TooltipLabel.Text = $"{baseDesc}\n当前耐久：{Amount}";
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
        if (power == this) UpdateTooltipLabel();
    }

    // ★ 新增：战斗结束时自动清理幻境残留
    public override async Task AfterCombatEnd(CombatRoom room)
    {
        await base.AfterCombatEnd(room);
        // 强制摧毁 UI 节点，并从管理器中移除自身
        if (IllusionNode != null && GodotObject.IsInstanceValid(IllusionNode))
        {
            IllusionNode.QueueFree();
            IllusionNode = null;
        }
        Manager?.RemoveIllusionPower(this);
    }
}