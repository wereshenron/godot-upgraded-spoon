using System;
using Godot;
using upgradedspoon.globals;
using upgradedspoon.traits;
using upgradedspoon.types;

namespace upgradedspoon.holdables.util;

public partial class Flashlight : Holdable
{
	[Export] public Vector3 AimOffset;
	[Export] public float AdsTime;
	[Export] public SpotLight3D Light;

	private double _aimBlend;
	private RayCast3D _aimRaycast;
	private Vector3 _direction = Vector3.Zero;

	private bool _isShining = false;
	private bool _shiningChangeQueued = true;

	public override void _Ready()
	{
		base._Ready();
		
		_aimRaycast = GameManager.Instance.Player.Camera.AimRaycast;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (!_shiningChangeQueued) return;

		Light.LightEnergy = _isShining ? 1.0f : 0.0f;
		_shiningChangeQueued = false;
	}

	public override void _PhysicsProcess(double delta)
	{
		var target = IsAiming ? 1f : 0f;
		var step = AdsTime > 0 ? (float)(delta / AdsTime) : 1f;
		_aimBlend = Mathf.MoveToward(_aimBlend, target, step);
	}

	public override Vector3 GetHoldOffset() => HoldOffset.Lerp(AimOffset, (float)_aimBlend);

	public override double GetFollowSpeed(double baseSpeed)
	{
		return IsAiming ? AimFollowSpeed : baseSpeed;
	}

	public override void OnAction(UseSlot slot, InputPhase phase, double delta, AimContext ctx)
	{
		switch (slot, phase)
		{
			case (UseSlot.Primary, InputPhase.Pressed):
				_isShining = !_isShining;
				_shiningChangeQueued = true;
				break;
			
			case (UseSlot.Secondary, InputPhase.Pressed):
				IsAiming = true;
				SignalBus.Instance.RaiseShowReticle(false);
				break;
			
			case (UseSlot.Secondary, InputPhase.Released):
				IsAiming = false;
				SignalBus.Instance.RaiseShowReticle(true);
				break;
		};
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
	
	public override void UpdateHold(Node3D holdPivot, double delta)
	{
		// _aimBlend = 1.0f; // Testing always aiming
		_aimBlend = Mathf.MoveToward(_aimBlend, IsAiming ? 1.0 : 0.0, delta / AdsTime);

		var forward = -holdPivot.GlobalBasis.Z;
		var right = holdPivot.GlobalBasis.X;
		var up = holdPivot.GlobalBasis.Y;
		var offset = GetHoldOffset();

		var holdTarget = holdPivot.GlobalPosition
						 + forward * offset.Z
						 + right * offset.X
						 + up * offset.Y;
		
		_direction = -_aimRaycast.GlobalBasis.Z.Normalized();

		var targetBasis = Basis.LookingAt(_direction, holdPivot.GlobalBasis.Y);
		var mass = Body.GetMass();
		var speed = GetFollowSpeed(BaseFollowSpeed);
		var followSpeed = speed / (1.0 + mass * MassInfluence);

		var clamp = (float)Mathf.Clamp(followSpeed * delta, 0.0, 1.0);

		Body.GlobalPosition = Body.GlobalPosition
			.Lerp(holdTarget, clamp);

		Body.GlobalBasis = Body.GlobalBasis
			.Slerp(targetBasis, clamp);
	}
}
