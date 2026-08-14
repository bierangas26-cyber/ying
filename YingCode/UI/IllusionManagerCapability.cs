using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib.Scaffolding.Godot;
using System.Collections.Generic;
using Yingmod.Ying.Patch;
using Yingmod.Ying.Powers;
using System;
using System.Linq;

namespace Yingmod.Ying.Capabilities;

[RegisterModelCapability]
public class IllusionManagerCapability : CharacterCapability
{
    private readonly List<IllusionPower> _illusionPowers = new();
    private Player? _ownerPlayer;

    private static readonly Dictionary<ulong, IllusionManagerCapability> _instances = new();

    public static IllusionManagerCapability? GetForPlayer(Player player)
    {
        _instances.TryGetValue(player.NetId, out var manager);
        return manager;
    }

    public void SetOwnerPlayer(Player player)
    {
        if (_ownerPlayer != null && _ownerPlayer.NetId != player.NetId)
        {
            foreach (var illusion in _illusionPowers.ToList())
            {
                if (illusion.IllusionNode != null && GodotObject.IsInstanceValid(illusion.IllusionNode))
                    RemoveIllusionUI(illusion.IllusionNode);
            }
            _illusionPowers.Clear();
        }
        _ownerPlayer = player;
        _instances[player.NetId] = this;
    }

    public IReadOnlyList<IllusionPower> Illusions => _illusionPowers;

    private Control? GetContainer()
    {
        if (_ownerPlayer == null) return null;
        return IllusionUIPatch.GetContainer(_ownerPlayer.NetId);
    }

    public Control? CreateAndAddIllusionUI(string cardImagePath, int durability)
    {
        var container = GetContainer();
        if (container == null) return null;

        var illusionUI = RitsuGodotNodeFactories.CreateFromScenePath<Control>("res://Ying/Scenes/IllusionNode.tscn");
        if (illusionUI == null) return null;

        illusionUI.CustomMinimumSize = new Vector2(100, 100);
        illusionUI.Size = new Vector2(100, 100);
        illusionUI.MouseFilter = Control.MouseFilterEnum.Stop;
        StripMouseFilterRecursive(illusionUI);

        var illustrationNode = illusionUI.GetNodeOrNull<TextureRect>("Mask/Illustration");
        var durabilityLabel = illusionUI.GetNodeOrNull<Label>("DurabilityLabel");

        if (illustrationNode != null)
            illustrationNode.Texture = ResourceLoader.Load<Texture2D>(cardImagePath);
        if (durabilityLabel != null)
            durabilityLabel.Text = durability.ToString();

        container.AddChild(illusionUI);
        RefreshLayoutAnimated();
        return illusionUI;
    }

    public void RemoveIllusionUI(Control illusionNode)
    {
        var container = GetContainer();
        if (container == null || illusionNode == null) return;

        // 确保节点仍然有效
        if (GodotObject.IsInstanceValid(illusionNode) && illusionNode.IsInsideTree())
            container.RemoveChild(illusionNode);

        if (GodotObject.IsInstanceValid(illusionNode))
            illusionNode.QueueFree();

        RefreshLayoutAnimated();
    }

    public void AddIllusionPower(IllusionPower power)
    {
        if (_ownerPlayer == null || power?.Owner?.Player == null) return;
        if (power.Owner.Player.NetId != _ownerPlayer.NetId) return;
        if (power.Manager != this) return;

        if (!_illusionPowers.Contains(power))
        {
            _illusionPowers.Insert(0, power);
            RefreshLayoutAnimated();
            BindTooltip(power);
        }
    }

    public void RemoveIllusionPower(IllusionPower power)
    {
        if (_illusionPowers.Remove(power))
            RefreshLayoutAnimated();
    }

    private void StripMouseFilterRecursive(Node parent)
    {
        foreach (var child in parent.GetChildren())
        {
            if (child is Control ctrl)
                ctrl.MouseFilter = Control.MouseFilterEnum.Ignore;
            StripMouseFilterRecursive(child);
        }
    }

    private void BindTooltip(IllusionPower power)
    {
        var node = power.IllusionNode;
        if (node == null || !GodotObject.IsInstanceValid(node))
            return;

        if (node.GetNodeOrNull<Label>("TooltipLabel") != null)
            return;

        var tooltipLabel = new Label
        {
            Name = "TooltipLabel",
            Visible = false,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            CustomMinimumSize = new Vector2(160, 0),
            Position = new Vector2(0, -100),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };

        var styleBox = new StyleBoxFlat { BgColor = new Color(0, 0, 0, 0.85f) };
        styleBox.SetCornerRadiusAll(4);
        tooltipLabel.AddThemeStyleboxOverride("normal", styleBox);
        tooltipLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1, 1));
        tooltipLabel.AddThemeFontSizeOverride("font_size", 13);

        string baseDesc = power.Description.GetFormattedText();
        tooltipLabel.Text = $"{baseDesc}\n当前耐久：{power.Amount}";

        node.AddChild(tooltipLabel);

        node.MouseEntered += () =>
        {
            if (GodotObject.IsInstanceValid(tooltipLabel))
                tooltipLabel.Visible = true;
        };
        node.MouseExited += () =>
        {
            if (GodotObject.IsInstanceValid(tooltipLabel))
                tooltipLabel.Visible = false;
        };

        power.TooltipLabel = tooltipLabel;
    }

    private void RefreshLayoutAnimated()
    {
        var container = GetContainer();
        if (container == null) return;

        List<Control> nodes = new();
        foreach (var power in _illusionPowers)
        {
            if (power.IllusionNode != null && GodotObject.IsInstanceValid(power.IllusionNode))
                nodes.Add(power.IllusionNode);
        }

        if (nodes.Count == 0) return;

        var tween = container.GetTree()?.CreateTween();
        if (tween == null) return;
        tween.SetParallel(true);

        float baseOffsetX = 55f;
        float baseOffsetY = 55f;
        float fixedScale = 0.8f;
        Vector2 currentPos = Vector2.Zero;

        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            node.LayoutMode = 1;
            node.AnchorLeft = 0f; node.AnchorTop = 0f;
            node.AnchorRight = 0f; node.AnchorBottom = 0f;

            float scale = (i == 0) ? 1.0f : fixedScale;
            Vector2 targetScale = new(scale, scale);

            if (i > 0)
            {
                float prevScale = (i - 1 == 0) ? 1.0f : fixedScale;
                float moveX = baseOffsetX * (prevScale + scale) / 2f;
                float moveY = baseOffsetY * (prevScale + scale) / 2f;

                if (i % 2 == 1)
                    currentPos += new Vector2(moveX, -moveY);
                else
                    currentPos += new Vector2(-moveX, -moveY);
            }

            node.ZIndex = -i;
            tween.TweenProperty(node, "position", currentPos, 0.3f)
                 .SetTrans(Tween.TransitionType.Back)
                 .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(node, "scale", targetScale, 0.3f)
                 .SetTrans(Tween.TransitionType.Back)
                 .SetEase(Tween.EaseType.Out);
        }
    }
}