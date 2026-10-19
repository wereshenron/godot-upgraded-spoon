using System;
using Godot;
using upgradedspoon.globals;
using upgradedspoon.models.bullets;
using upgradedspoon.state;
using upgradedspoon.traits.shootable.gun_settings;
using upgradedspoon.traits.shootable.states;
using upgradedspoon.types;

namespace upgradedspoon.traits.shootable;

public partial class Shootable : Holdable
{
	[Export] public ShootableStateMachine StateMachine { get; set; }
	[Export] public Material TracerMaterial { get; set; }
	[Export] public Vector3 AimOffset { get; set; }
	[Export] public GunSettings GunSettings { get; set; }
	[Export] public float AimLength { get; set; } = 1000.0f;
	[Export] public PackedScene Marker { get; set; }

	/// Latest aim from the interactor; states read this when firing.
	public AimContext CurrentAim { get; private set; }

	public int Ammo { get; private set; }
	public bool CanReload => Ammo < GunSettings.MagazineSize;

	private Node3D _bulletPivot;

	/// 0 = hold_offset, 1 = aim_offset
	private double _aimBlend;

	private uint _initialLayer;
	private uint _initialMask;
	private Vector3 _targetRecoilOffset;
	private Vector3 _currentRecoilOffset;
	private RayCast3D _aimRaycast;
	private Vector3 _shootAimPoint;
	private Vector3 _direction = Vector3.Zero;

	private ShootableState CurrentState => StateMachine.CurrentState as ShootableState;

	public override void _Ready()
	{
		base._Ready();

		Ammo = GunSettings.MagazineSize;

		_bulletPivot = Body.GetNode<Node3D>("BulletPivot");
		_initialLayer = Body.CollisionLayer;
		_initialMask = Body.CollisionMask;
		_aimRaycast = GetViewport().GetCamera3D().GetNode("AimRaycast") as RayCast3D;

		StateMachine.AddState("Firing", new ShootableFiringState());
		StateMachine.AddState("Idle", new ShootableIdleState());
		StateMachine.Enter("Idle");

		Grabbed += (_, _) => OnGrabbed();
		Released += (_, _) => OnReleased();
	}

	public override void _PhysicsProcess(double delta)
	{
		var target = IsAiming ? 1f : 0f;
		var step = GunSettings.AdsTime > 0 ? (float)(delta / GunSettings.AdsTime) : 1f;
		_aimBlend = Mathf.MoveToward(_aimBlend, target, step);
	}

	public override void UpdateHold(Node3D holdPivot, double delta)
	{
		_aimBlend = Mathf.MoveToward(_aimBlend, IsAiming ? 1.0 : 0.0, delta / GunSettings.AdsTime);

		_targetRecoilOffset = _targetRecoilOffset.MoveToward(Vector3.Zero, GunSettings.RecoilSpeed * (float)delta);
		
		var weight = 1.0f - MathF.Exp(-GunSettings.Snap * (float)delta);
		_currentRecoilOffset = _currentRecoilOffset.Lerp(_targetRecoilOffset, weight);

		if (_currentRecoilOffset.DistanceSquaredTo(_targetRecoilOffset) < 0.0001f)
			_currentRecoilOffset = _targetRecoilOffset;

		var forward = -holdPivot.GlobalBasis.Z;
		var right = holdPivot.GlobalBasis.X;
		var up = holdPivot.GlobalBasis.Y;
		var offset = GetHoldOffset();

		var holdTarget = holdPivot.GlobalPosition
						 + forward * (offset.Z + _currentRecoilOffset.Z)
						 + right * (offset.X + _currentRecoilOffset.X)
						 + up * (offset.Y + _currentRecoilOffset.Y);

		var aimOrigin = _aimRaycast.GlobalPosition;
		var aimDir = -_aimRaycast.GlobalBasis.Z;
		var aimPoint = aimOrigin + aimDir * AimLength;

		_shootAimPoint = _aimRaycast.IsColliding() ? _aimRaycast.GetCollisionPoint() : Vector3.Zero;
		_direction = (aimPoint - Body.GlobalPosition).Normalized();

		var targetBasis = Basis.LookingAt(_direction, holdPivot.GlobalBasis.Y);
		var yawBasis = new Basis(Vector3.Up, _currentRecoilOffset.Y);
		var aimWithYaw = yawBasis * targetBasis;

		var pitchBasis = new Basis(aimWithYaw.X, _currentRecoilOffset.X);
		var targetBasisFinal = pitchBasis * aimWithYaw;

		var mass = Body.GetMass();
		var speed = GetFollowSpeed(BaseFollowSpeed);
		var followSpeed = speed / (1.0 + mass * MassInfluence);

		var clamp = (float)Mathf.Clamp(followSpeed * delta, 0.0, 1.0);

		Body.GlobalPosition = Body.GlobalPosition
			.Lerp(holdTarget, clamp);

		Body.GlobalBasis = Body.GlobalBasis
			.Slerp(targetBasisFinal, clamp);
	}

	public void Shoot()
	{
		var spawnPos = _bulletPivot.GlobalPosition;
		var toAim = CurrentAim.AimPoint - spawnPos;
		var direction = toAim.LengthSquared() > 0.01f
			? toAim.Normalized()
			: CurrentAim.CameraForward;
		
		Ammo--;
		
		SignalBus.Instance.RaiseAmmoChanged(new AmmoState(Ammo, 0, GunSettings.MagazineSize));
		ApplyRecoilKick();
		LaunchBullet(direction, CurrentAim.AimPoint);
		// SpawnMarker(CurrentAim.AimPoint);
	}

	public void RefillMagazine() => Ammo = GunSettings.MagazineSize;


	private void ApplyRecoilKick()
	{
		var min = GunSettings.MinRecoilAmount;
		var max = GunSettings.RecoilAmount;

		var applied = new Vector3(
			(float)Random.Shared.NextDouble() * (max.X - min.X) + min.X,
			(float)Random.Shared.NextDouble() * (max.Y - min.Y) + min.Y,
			(float)Random.Shared.NextDouble() * (max.Z - min.Z) + min.Z);

		SignalBus.Instance.RaiseRecoilKicked(applied, GunSettings.MaxRecoilOffset);
		_targetRecoilOffset += applied;
	}

	private void LaunchBullet(Vector3 direction, Vector3 aimPoint)
	{
		var bullet = GunSettings.BulletScene.Instantiate<Bullet>();
		GetTree().Root.AddChild(bullet);
		bullet.GlobalPosition = _bulletPivot.GlobalPosition;
		bullet.GlobalBasis = GlobalBasis;
		bullet.Velocity = direction * GunSettings.FireVelocity;
		bullet.Mass = GunSettings.ReferenceBulletMass;
		bullet.CollisionPoint = aimPoint;
	}

	// Input from the interactor
	public override void OnAction(UseSlot slot, InputPhase phase, double delta, AimContext ctx)
	{
		CurrentAim = ctx;

		if (slot == UseSlot.Secondary)
			HandleAim(phase);

		CurrentState?.OnAction(slot, phase, delta, ctx);
	}

	private void HandleAim(InputPhase phase)
	{
		switch (phase)
		{
			case InputPhase.Pressed:
				IsAiming = true;
				SignalBus.Instance.RaiseShowReticle(false);
				break;
			case InputPhase.Released:
				IsAiming = false;
				SignalBus.Instance.RaiseShowReticle(true);
				break;
			case InputPhase.Held:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
		}
	}

	public override Vector3 GetHoldOffset() => HoldOffset.Lerp(AimOffset, (float)_aimBlend);

	private void OnGrabbed()
	{
		Body.Freeze = true;
		Body.CollisionLayer = 0;
		Body.CollisionMask = 0;
		
		SignalBus.Instance.RaiseLookedAway();
		SignalBus.Instance.RaiseAmmoShown(this, true);
	}

	private void OnReleased()
	{
		CurrentState?.Cancelled();
		IsAiming = false;
		SignalBus.Instance.RaiseShowReticle(true);

		Body.Freeze = false;
		Body.CollisionLayer = _initialLayer;
		Body.CollisionMask = _initialMask;
		
		SignalBus.Instance.RaiseLookedAway();
		SignalBus.Instance.RaiseAmmoShown(this, false);
	}

	private void SpawnMarker(Vector3? endPoint = null)
	{
		// Checks if it has a value and assigns it to 'pos' at the same time
		if (endPoint is not { } pos) return;
	
		var instance = Marker.Instantiate<Node3D>();
		GetTree().Root.AddChild(instance);
		instance.GlobalPosition = pos;
	}
}
