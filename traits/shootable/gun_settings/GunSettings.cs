using Godot;

namespace upgradedspoon.traits.shootable.gun_settings;

[GlobalClass]
public partial class GunSettings : Resource
{
	[ExportCategory("Firing")]
	[Export] public Vector3 RecoilAmount { get; set; }
	[Export] public Vector3 MinRecoilAmount { get; set; }
	[Export] public Vector3 MaxRecoilOffset { get; set; }
	[Export] public float Snap { get; set; }
	[Export] public float RecoilSpeed { get; set; }
	[Export] public float FireRate { get; set; }
	[Export] public float FireVelocity { get; set; }
	[Export] public float ReferenceBulletMass { get; set; }
	[Export] public bool FullAuto { get; set; }
	[Export] public PackedScene BulletScene { get; set; }
	[Export] public float Damage { get; set; }
	[Export] public int MagazineSize { get; set; }
	
	public GunSettings() {}
}
