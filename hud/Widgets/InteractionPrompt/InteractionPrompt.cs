using System;
using Godot;
using upgradedspoon.globals;
using upgradedspoon.traits;

namespace upgradedspoon.hud.Widgets.InteractionPrompt;

public partial class InteractionPrompt : HudWidget
{
    [Export] public float HideTimeout = 0.08f;
    [Export] public float ShowTimeout = 0.12f;
    
    [Export] private Label _actionLabel;
    [Export] private Label _itemLabel;

    public override void _Ready()
    {
        Visible = false;

        SignalBus.Instance.InteractableSeen += OnTargetSeen;
        SignalBus.Instance.LookedAway += OnLookedAway;
    }

    public override void _ExitTree()
    {
        SignalBus.Instance.InteractableSeen -= OnTargetSeen;
        SignalBus.Instance.LookedAway -= OnLookedAway;
    }

    private void OnTargetSeen(object sender, Interactable target)
    {
        _itemLabel.Text = target.Title;
        _actionLabel.Text = target.Action;
        _itemLabel.Visible = target.Title != "";
        Visible = true;
        FadeTo(1f, ShowTimeout);
    }

    private void OnLookedAway(object sender, EventArgs e)
        => FadeTo(0f, HideTimeout, () => Visible = false);
}