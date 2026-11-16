using System;
using Godot;
using upgradedspoon.globals;
using upgradedspoon.player.interaction;
using Interactable = upgradedspoon.traits.interactable.Interactable;

namespace upgradedspoon.hud.Widgets.InteractionPrompt;

public partial class InteractionPrompt : HudWidget
{
	[Export] public float HideTimeout = 0.08f;
	[Export] public float ShowTimeout = 0.12f;

	[Export] private Label _actionLabel;
	[Export] private Label _itemLabel;

	private Interactable _interactable;
	private Interactor _interactor;

	public override void _Ready()
	{
		Visible = false;

		SignalBus.Instance.InteractableSeen += OnTargetSeen;
		SignalBus.Instance.LookedAway += OnLookedAway;
	}

	public override void _Process(double delta)
	{
		if (_interactable == null) return;

		GlobalPosition = GetViewport().GetCamera3D().UnprojectPosition(_interactable.UiOffset.GlobalPosition);
	}


	private void OnTargetSeen(object sender, InteractableSeenArgs e)
	{
		_interactable = e.Interactable;
		_interactor = e.Interactor;

		// Text changes
		_itemLabel.Text = _interactable.Title;
		_actionLabel.Text = _interactable.Action;
		_itemLabel.Visible = _interactable.Title != "";

		// Visibility transition
		Visible = true;
		var mod = Modulate;
		mod.A = 0.0f;
		Modulate = mod;

		FadeTo(1f, ShowTimeout);

		GlobalPosition = GetViewport().GetCamera3D().UnprojectPosition(_interactable.UiOffset.GlobalPosition);
	}

	private void OnLookedAway(object sender, EventArgs e)
	{
		FadeTo(0f, HideTimeout, () => Visible = false);

		_interactable = null;
	}


	public override void _ExitTree()
	{
		SignalBus.Instance.InteractableSeen -= OnTargetSeen;
		SignalBus.Instance.LookedAway -= OnLookedAway;
	}
}
