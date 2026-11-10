using Godot;
using upgradedspoon.globals;

namespace upgradedspoon.hud.Widgets.QuitButton;

public partial class QuitButton : HudWidget
{
    [Export] public float ShowTimeout = 0.12f;

    public override void _Ready()
    {
        SignalBus.Instance.PauseStarted += OnPauseStarted;
        // ButtonCl
    }

    private void OnPauseStarted(object sender, bool state)
    {
        FadeTo(state ? 1.0f : 0f, ShowTimeout, () => Visible = state);
    }
}