using Godot;
using upgradedspoon.globals;

namespace upgradedspoon.hud.Widgets.Reticle;

public partial class Reticle : HudWidget
{
	[Export] public float ShowTimeout = 0.12f;
	
	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Ignore;
		SignalBus.Instance.ShowReticle += OnShowReticle;
	}

	private void OnShowReticle(object sender, bool state)
	{
		FadeTo(state ? 1.0f : 0f, ShowTimeout, () => Visible = state);
	}
}
