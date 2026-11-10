using System;
using Godot;
using upgradedspoon.globals;
using upgradedspoon.traits;

namespace upgradedspoon.hud.Widgets.Reticle;

public partial class Reticle : HudWidget
{
	[Export] public RichTextLabel Small { get; set; }
	[Export] public RichTextLabel Bold { get; set; }
	[Export] public float ShowTimeout = 0.12f;
	
	public override void _Ready()
	{
		Bold.Visible = false;
		MouseFilter = MouseFilterEnum.Ignore;
		
		SignalBus.Instance.ShowReticle += OnShowReticle;
		SignalBus.Instance.InteractableSeen += OnInteractableSeen;
		SignalBus.Instance.LookedAway += OnLookedAway;
	}

	private void OnShowReticle(object sender, bool state)
	{
		FadeTo(state ? 1.0f : 0f, ShowTimeout, () => Visible = state);
	}
	
	

	private void OnLookedAway(object sender, EventArgs e)
	{
		Bold.Visible = false;
	}

	private void OnInteractableSeen(object sender, Interactable e)
	{
		Bold.Visible = true;
	}
}
