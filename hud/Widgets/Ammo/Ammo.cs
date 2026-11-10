using Godot;
using upgradedspoon.globals;
using upgradedspoon.traits.shootable;
using upgradedspoon.types;

namespace upgradedspoon.hud.Widgets.Ammo;

public partial class Ammo : HudWidget
{
	[Export] public RichTextLabel Loaded { get; set; }
	[Export] public RichTextLabel MagSize { get; set; }

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

	private void OnAmmoShown(object sender, bool e)
	{
		if (sender is not Shootable shootable) return;
		_holder = shootable;
		
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
