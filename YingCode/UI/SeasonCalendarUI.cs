using Godot;

namespace Yingmod.Ying.UI;

public partial class SeasonCalendarUI : Control
{
	private AnimationPlayer _animPlayer;
	private int _currentSeason = 1; // 1:春 2:夏 3:秋 4:冬

	public override void _Ready()
	{
		_animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		// 强制重置到春季显示，不播动画
		ResetToSeason(1);
	}

	private void ResetToSeason(int season)
	{
		string[] panelNames = { "SpringBgPanel", "SummerBgPanel", "AutumnBgPanel", "WinterBgPanel" };
		string[] labelNames = { "SpringLabel", "SummerLabel", "AutumnLabel", "WinterLabel" };

		for (int i = 0; i < 4; i++)
		{
			var panel = GetNodeOrNull<Panel>(panelNames[i]);
			if (panel != null)
			{
				panel.Visible = (i == season - 1);
				panel.Position = Vector2.Zero;
				panel.Size = new Vector2(400, 200);
			}
			var label = GetNodeOrNull<Label>(labelNames[i]);
			if (label != null)
			{
				label.Visible = (i == season - 1);
				label.Modulate = new Color(1, 1, 1, 1);
			}
		}
		_currentSeason = season;
	}

	public void UpdateSeason(int newSeason)
	{
		if (newSeason == _currentSeason) return;

		string animName = GetAnimName(_currentSeason, newSeason);
		bool animationExists = !string.IsNullOrEmpty(animName) && _animPlayer?.HasAnimation(animName) == true;

		if (animationExists)
		{
			_animPlayer.Play(animName);
		}
		else
		{
			// 动画缺失时，硬切换到新季节的显示状态
			ResetToSeason(newSeason);
		}

		_currentSeason = newSeason; // 无论如何都更新当前季节记录
	}

	private static string GetAnimName(int from, int to) => (from, to) switch
	{
		(1, 2) => "spring_to_summer",
		(2, 3) => "summer_to_autumn",
		(3, 4) => "autumn_to_winter",
		(4, 1) => "winter_to_spring",
		_ => null
	};
}
