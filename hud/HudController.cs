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
	private HBoxContainer _interactionLabel;
	private Label _itemLabel;
	private RichTextLabel _boldReticle;
	private RichTextLabel _reticle;
	private string _targetAction = "";
	private string _targetTitle = "";

	private Tween _tween;

	public override void _Ready()
	{
		_interactionLabel = GetNode<HBoxContainer>("Root/InteractionPrompt");
		_reticle = GetNode<RichTextLabel>("Root/Cursor");
		_boldReticle = GetNode<RichTextLabel>("Root/BoldCursor");
		_actionLabel = GetNode<Label>("Root/InteractionPrompt/ActionLabel");
		_itemLabel = GetNode<Label>("Root/InteractionPrompt/ItemLabel");

		_interactionLabel.Visible = false;
		_boldReticle.Visible = false;
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

		_interactionLabel.Visible = true;
		_boldReticle.Visible = true;

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
		_tween.TweenProperty(_interactionLabel, "modulate:a", targetAlpha, duration);
		_tween.TweenProperty(_boldReticle, "modulate:a", targetAlpha, duration);
	}

	private void OnHideTweenFinished()
	{
		_interactionLabel.Visible = false;
		_boldReticle.Visible = false;
	}

	private void OnShowReticle(object sender, bool state)
	{
		_reticle.Visible = state;
	}
}
