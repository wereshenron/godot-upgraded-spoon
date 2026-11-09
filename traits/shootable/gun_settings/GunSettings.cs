using Godot;

namespace upgradedspoon.traits.shootable.gun_settings;

/// <summary>
/// Per-gun tuning values. Create one as a resource and assign it to a <c>Shootable</c>.
/// </summary>
[GlobalClass]
public partial class GunSettings : Resource
{
	/// <summary>
	/// Maximum recoil kick applied per shot. Each axis is randomized between
	/// <see cref="MinRecoilAmount"/> and this value.
	/// </summary>
	[ExportCategory("Firing")]
	[Export] public Vector3 RecoilAmount { get; set; }

	/// <summary>
	/// Minimum recoil kick applied per shot. Each axis is randomized between
	/// this value and <see cref="RecoilAmount"/>.
	/// </summary>
	[Export] public Vector3 MinRecoilAmount { get; set; }

	/// <summary>
	/// Limit on accumulated recoil. Sent along with each kick to anything listening
	/// to the recoil signal (e.g. camera recoil).
	/// </summary>
	[Export] public Vector3 MaxRecoilOffset { get; set; }

	/// <summary>
	/// How quickly the visible recoil follows its target. Higher values are snappier,
	/// lower values feel smoother and floatier.
	/// </summary>
	[Export] public float Snap { get; set; }

	/// <summary>
	/// How fast the recoil target returns to zero after firing, in units per second.
	/// Higher values mean faster recovery.
	/// </summary>
	[Export] public float RecoilSpeed { get; set; }

	/// <summary>
	/// How often the gun can fire. Read by the firing state.
	/// </summary>
	[Export] public float FireRate { get; set; }

	/// <summary>
	/// Muzzle velocity applied to each bullet, in meters per second.
	/// </summary>
	[Export] public float FireVelocity { get; set; }

	/// <summary>
	/// Mass assigned to each spawned bullet. Affects the bullet's physics behavior
	/// and the impulse it applies on impact.
	/// </summary>
	[Export] public float ReferenceBulletMass { get; set; }

	/// <summary>
	/// If true, holding the fire button keeps shooting. If false, one shot per press.
	/// </summary>
	[Export] public bool FullAuto { get; set; }

	/// <summary>
	/// Scene instantiated for each shot. Its root must be a <c>Bullet</c>.
	/// </summary>
	[Export] public PackedScene BulletScene { get; set; }

	/// <summary>
	/// Base damage dealt by a single bullet.
	/// </summary>
	[Export] public float Damage { get; set; }

	/// <summary>
	/// Number of rounds in a full magazine. Also, the ammo count after a reload.
	/// </summary>
	[Export] public int MagazineSize { get; set; }

	/// <summary>
	/// Time in seconds to move from hip-fire to fully aimed. Set to 0 for an instant transition.
	/// </summary>
	[Export] public float AdsTime { get; set; } = 0.15f;

	public GunSettings() {}
}
