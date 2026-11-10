using Godot;
using upgradedspoon.globals;
using upgradedspoon.traits;
using upgradedspoon.traits.shootable;
using upgradedspoon.types;

namespace upgradedspoon.hud.Widgets.Ammo;

public partial class Ammo : HudWidget
{
	[Export] public RichTextLabel Loaded { get; set; }
	[Export] public RichTextLabel MagSize { get; set; }
	
	[Export] public float HideTimeout = 0.08f;
	[Export] public float ShowTimeout = 0.40f;

	private Shootable _holder;
	public override void _Ready()
	{
		Visible = false;
		
		SignalBus.Instance.AmmoShown += OnAmmoShown;
		SignalBus.Instance.AmmoChanged += OnAmmoChanged;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_holder is null) return;

		GlobalPosition = GetViewport().GetCamera3D().UnprojectPosition(_holder.UiOffset.GlobalPosition);
	}

	private void OnAmmoChanged(object sender, AmmoState state)
	{
		SetAmmoState(state.Loaded, state.MagSize);
	}

	private void OnAmmoShown(object sender, bool shown)
	{
		if (sender is not Shootable shootable) return;

		if (shown)
		{
			Visible = true;
			FadeTo(1f, ShowTimeout);
		}
		else
		{
			FadeTo(0f, HideTimeout, () => Visible = false);
		}
		
		SetAmmoState(shootable.Ammo, shootable.GunSettings.MagazineSize);
		
		_holder = shootable;
	}

	// private void SetAmmoState(AmmoState state) TODO - Add reserve to UI
	private void SetAmmoState(int loaded, int magSize)
	{
		Loaded.Text = loaded.ToString();
		MagSize.Text = magSize.ToString();
	}
}
