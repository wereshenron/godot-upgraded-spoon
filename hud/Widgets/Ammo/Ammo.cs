using Godot;
using upgradedspoon.globals;
using upgradedspoon.traits.shootable;

namespace upgradedspoon.hud.Widgets.Ammo;

public partial class Ammo : HudWidget
{
	[Export] public RichTextLabel Loaded { get; set; }
	[Export] public RichTextLabel MagSize { get; set; }
	public override void _Ready()
	{
		Visible = false;
		
		SignalBus.Instance.AmmoShown += OnAmmoShown;
		SignalBus.Instance.AmmoChanged += OnAmmoChanged;
	}

	private void OnAmmoChanged(object sender, AmmoState state)
	{
		SetAmmoState(state.Loaded, state.MagSize);
	}

	private void OnAmmoShown(object sender, bool e)
	{
		if (sender is not Shootable shootable) return;
		
		Visible = e;
		SetAmmoState(shootable.Ammo, shootable.GunSettings.MagazineSize);
		// TODO - Add reserve 
	}

	// private void SetAmmoState(AmmoState state) TODO - Add reserve to UI
	private void SetAmmoState(int loaded, int magSize)
	{
		Loaded.Text = loaded.ToString();
		MagSize.Text = magSize.ToString();
	}
}
