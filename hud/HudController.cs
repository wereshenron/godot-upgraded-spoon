using System;
using Godot;
using upgradedspoon.globals;
using upgradedspoon.traits;

namespace upgradedspoon.hud;

public partial class HudController : CanvasLayer
{
	[Export] public float HideTimeout = 0.08f;
	[Export] public float ShowTimeout = 0.12f;
	private Label _actionLabel;
	private RichTextLabel _boldCursor;

	private HBoxContainer _instructionLabel;
	private Label _itemLabel;
	private RichTextLabel _reticle;
	private string _targetAction = "";
	private string _targetTitle = "";

	private Tween _tween;

	public override void _Ready()
	{
		_instructionLabel = GetNode<HBoxContainer>("Root/HBoxContainer");
		_reticle = GetNode<RichTextLabel>("Root/Cursor");
		_boldCursor = GetNode<RichTextLabel>("Root/BoldCursor");
		_actionLabel = GetNode<Label>("Root/HBoxContainer/ActionLabel");
		_itemLabel = GetNode<Label>("Root/HBoxContainer/ItemLabel");

		_instructionLabel.Visible = false;
		_boldCursor.Visible = false;
		_itemLabel.Visible = false;

		SignalBus.Instance.InteractableSeen += OnTargetSeen;
		SignalBus.Instance.LookedAway += OnLookedAway;
		SignalBus.Instance.ShowReticle += OnShowReticle;
	}

	private void OnTargetSeen(object sender, Interactable target)
	{
		StartFade(1.0f, ShowTimeout);

		_targetTitle = target.Title;
		_targetAction = target.Action;

		_itemLabel.Text = _targetTitle;
		_actionLabel.Text = _targetAction;

		_instructionLabel.Visible = true;
		_boldCursor.Visible = true;

		_itemLabel.Visible = _itemLabel.Text != "";
	}

	private void OnLookedAway(object sender, EventArgs e)
	{
		StartFade(0.0f, HideTimeout);
		_tween.Chain().TweenCallback(Callable.From(OnHideTweenFinished));
	}

	private void StartFade(float targetAlpha, float duration)
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_instructionLabel, "modulate:a", targetAlpha, duration);
		_tween.TweenProperty(_boldCursor, "modulate:a", targetAlpha, duration);
	}

	private void OnHideTweenFinished()
	{
		_instructionLabel.Visible = false;
		_boldCursor.Visible = false;
	}

	private void OnShowReticle(object sender, bool state)
	{
		_reticle.Visible = state;
	}
}
