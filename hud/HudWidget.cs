using System;
using Godot;

namespace upgradedspoon.hud;

public partial class HudWidget : Control
{
	private static readonly NodePath ModulateAlpha = "modulate:a";
	private Tween _tween;

	public override void _Ready()
	{
		base._Ready();
	}

	/// <param name="alpha">Current visibility of alpha channel</param>
	/// <param name="duration">Time to change</param>
	/// <param name="onFinished">Callback for after tween is done</param>
	protected void FadeTo(float alpha, float duration, Action onFinished = null)
	{
		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(this, ModulateAlpha, alpha, duration);
		if (onFinished != null)
			_tween.TweenCallback(Callable.From(onFinished));
	}
}
