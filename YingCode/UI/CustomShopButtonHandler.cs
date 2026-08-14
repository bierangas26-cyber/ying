using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.TopBar;
using Yingmod.Ying.UI;

namespace Yingmod.Ying.TopBar;

[RegisterOwnedTopBarButton(
    "custom_shop",
    IconPath = "res://Ying/Images/UI/shop_icon.png",
    ButtonOrder = 10)]
public class CustomShopButtonHandler : IModTopBarButtonHandler
{
    private ShopPanel? _panel;

    public void OnClick(ModTopBarButtonContext ctx)
    {
        if (ctx.Player == null) return;

        // 如果已打开，则关闭
        if (_panel != null && GodotObject.IsInstanceValid(_panel) && _panel.Visible)
        {
            ctx.CloseCapstoneScreen();
            return;
        }

        // 销毁可能残留的无效面板
        if (_panel != null)
        {
            if (GodotObject.IsInstanceValid(_panel))
                _panel.QueueFree();
            _panel = null;
        }

        // 创建全新面板
        var scene = ResourceLoader.Load<PackedScene>("res://Ying/Scenes/ShopPanel.tscn");
        if (scene == null) return;
        _panel = scene.Instantiate<ShopPanel>();

        // 面板关闭时的清理逻辑
        _panel.OnClose += () =>
        {
            if (_panel != null)
            {
                var parent = _panel.GetParent();
                if (parent != null)
                    parent.RemoveChild(_panel);
                _panel.QueueFree();
                _panel = null;
            }
            ctx.CloseCapstoneScreen();
        };

        ctx.OpenCapstoneScreen(_panel);
        _panel.Open(ctx.Player);
    }

    public bool IsVisible(ModTopBarButtonContext ctx)
    {
        if (CombatManager.Instance?.IsInProgress == true) return false;
        if (ctx.Player?.RunState == null) return false;
        return true;
    }

    public bool IsOpen(ModTopBarButtonContext ctx) =>
        _panel != null && GodotObject.IsInstanceValid(_panel) && _panel.Visible;

    public int GetCount(ModTopBarButtonContext ctx) => -1;
}