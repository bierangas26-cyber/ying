using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yingmod.Ying.UI;

public partial class ShopPanel : Control, ICapstoneScreen
{
    // ICapstoneScreen 必须实现的成员
    public NetScreenType ScreenType => NetScreenType.None;
    public bool UseSharedBackstop => false;
    public Control? DefaultFocusedControl => null;
    public void AfterCapstoneOpened() { }
    public void AfterCapstoneClosed() { }

    public event Action? OnClose;

    private Player? _currentPlayer;
    private Label _goldLabel = null!;
    private GridContainer _leftGrid = null!;
    private Button _drawButton = null!;
    private List<(RelicModel relic, int price)> _shopItems = new();
    private const int DrawCost = 100;

    public override void _Ready()
    {
        // 阻止鼠标事件穿透
        MouseFilter = MouseFilterEnum.Stop;

        _goldLabel = GetNode<Label>("GoldLabel");
        _leftGrid = GetNode<GridContainer>("LeftGrid");
        _drawButton = GetNode<Button>("DrawButton");
        var closeBtn = GetNode<Button>("CloseButton");

        _drawButton.Pressed += OnDrawClicked;
        closeBtn.Pressed += OnCloseButtonPressed;

        // 启用输入处理，以便在失去焦点时仍能接收 _Input 事件
        SetProcessInput(true);
    }

    public override void _Input(InputEvent @event)
    {
        // ESC 键关闭商店（作为后备方案）
        if (@event.IsActionPressed("ui_cancel") && Visible)
        {
            OnCloseButtonPressed();
            AcceptEvent();
        }
    }

    public void Open(Player player)
    {
        _currentPlayer = player;
        RefreshUI();
        RefreshShopItems();
        Visible = true;

        // 订阅屏幕上下文更新事件，实现自动关闭
        ActiveScreenContext.Instance.Updated += OnActiveScreenChanged;
        GrabFocus();
    }

    private void RefreshUI() => _goldLabel.Text = $"金币: {_currentPlayer?.Gold ?? 0}";

    private void RefreshShopItems()
    {
        if (_currentPlayer == null) return;

        // 清空旧按钮
        for (int i = _leftGrid.GetChildCount() - 1; i >= 0; i--)
            _leftGrid.GetChild(i).QueueFree();
        _shopItems.Clear();

        // ★ 固定种子盐值 0，商品在同一房间内保持不变，但跨房间自动变化
        uint rngState = CreateShopSeed(_currentPlayer, 0);

        var ancientRelics = ModelDb.RelicPool<EventRelicPool>()
            .AllRelics
            .Where(r => r.IsAllowed(_currentPlayer.RunState))
            .ToList();

        if (ancientRelics.Count == 0)
        {
            _leftGrid.AddChild(new Label { Text = "暂无先古遗物可售" });
            return;
        }

        var btnStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.2f, 0.1f, 0.3f, 0.9f),
            BorderColor = new Color(1, 0.8f, 0.2f),
            BorderWidthBottom = 2,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8
        };

        for (int i = 0; i < 9; i++)
        {
            var relic = ancientRelics[NextInt(ref rngState, ancientRelics.Count)];
            int price = NextInt(ref rngState, 200, 401); // 价格 200~400
            _shopItems.Add((relic, price));

            string desc = relic.DynamicDescription.GetRawText();

            var btn = new Button
            {
                Text = $"{relic.Title.GetFormattedText()}\n{price} 金币",
                TooltipText = desc,
                CustomMinimumSize = new Vector2(200, 80)
            };
            btn.AddThemeStyleboxOverride("normal", btnStyle);
            btn.Icon = relic.Icon;
            btn.ExpandIcon = true;

            int idx = i;
            btn.Pressed += () => OnBuy(idx);
            _leftGrid.AddChild(btn);
        }
    }

    private async void OnBuy(int index)
    {
        if (_currentPlayer == null || index >= _shopItems.Count) return;
        var (relic, price) = _shopItems[index];
        if (_currentPlayer.Gold >= price)
        {
            await PlayerCmd.LoseGold(price, _currentPlayer, GoldLossType.Spent);
            var mutable = relic.ToMutable();
            await RelicCmd.Obtain(mutable, _currentPlayer);

            // 联机同步
            RunManager.Instance.RewardSynchronizer.SyncLocalGoldLost(price);
            RunManager.Instance.RewardSynchronizer.SyncLocalObtainedRelic(mutable);

            RefreshUI();
            if (_leftGrid.GetChild(index) is Button btn)
            {
                btn.Disabled = true;
                btn.Text = "已购买";
            }
        }
    }

    private async void OnDrawClicked()
    {
        if (_currentPlayer == null || _currentPlayer.Gold < DrawCost) return;
        await PlayerCmd.LoseGold(DrawCost, _currentPlayer, GoldLossType.Spent);
        RefreshUI();

        // 抽卡使用独立种子，与商品分开，同样固定盐值，但在同一层内保持不变
        uint rngState = CreateShopSeed(_currentPlayer, 1000, includeGold: true);

        RelicRarity rarity = NextInt(ref rngState, 100) switch
        {
            < 50 => RelicRarity.Common,
            < 80 => RelicRarity.Uncommon,
            < 95 => RelicRarity.Rare,
            _ => RelicRarity.Ancient
        };

        var grabBag = _currentPlayer.RelicGrabBag;
        var drawn = grabBag.PullFromFront(rarity, _currentPlayer.RunState)
                    ?? _currentPlayer.RunState.SharedRelicGrabBag.PullFromFront(rarity, _currentPlayer.RunState);

        if (drawn != null)
        {
            var mutable = drawn.ToMutable();
            await RelicCmd.Obtain(mutable, _currentPlayer);
            RunManager.Instance.RewardSynchronizer.SyncLocalGoldLost(DrawCost);
            RunManager.Instance.RewardSynchronizer.SyncLocalObtainedRelic(mutable);
        }
    }

    private void OnCloseButtonPressed()
    {
        Close();
    }

    /// <summary>
    /// 屏幕上下文自动关闭：当商店可见且不再是当前活动屏幕时触发。
    /// </summary>
    private void OnActiveScreenChanged()
    {
        if (Visible && !ActiveScreenContext.Instance.IsCurrent(this))
        {
            Close();
        }
    }

    private void Close()
    {
        if (!Visible) return;

        Visible = false;
        SetProcessInput(false);
        ActiveScreenContext.Instance.Updated -= OnActiveScreenChanged;
        OnClose?.Invoke();
    }

    private static uint CreateShopSeed(Player player, int salt, bool includeGold = false)
    {
        uint state = player.RunState.Rng.Seed ^ (uint)salt;
        state ^= (uint)player.RunState.TotalFloor * 0x9E3779B9u;
        state ^= (uint)player.NetId;
        state ^= (uint)(player.NetId >> 32);
        if (includeGold)
            state ^= (uint)player.Gold * 0x85EBCA6Bu;
        return state == 0 ? 1u : state;
    }

    private static int NextInt(ref uint state, int exclusiveMax)
    {
        if (exclusiveMax <= 0) return 0;
        state = state * 1664525u + 1013904223u;
        return (int)(state % (uint)exclusiveMax);
    }

    private static int NextInt(ref uint state, int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive) return minInclusive;
        int range = maxExclusive - minInclusive;
        return minInclusive + NextInt(ref state, range);
    }
}