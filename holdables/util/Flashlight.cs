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

	private double _aimBlend;
	private RayCast3D _aimRaycast;
	private Vector3 _direction = Vector3.Zero;

	public override void _Ready()
	{
		base._Ready();
		
		_aimRaycast = GameManager.Instance.Player.Camera.AimRaycast;
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

	// public override void OnAction(UseSlot slot, InputPhase phase, double delta, AimContext ctx)
	// {
	// 	switch (slot, phase)
	// 	{
	// 		case (UseSlot.Primary, InputPhase.Pressed):
	// 			if (IsAiming)
	// 			{
	// 				Throw((ctx.AimPoint - Body.GlobalPosition).Normalized());
	// 			}
	//
	// 			IsAiming = false;
	// 			_chargeTime = 0.0f;
	// 			break;
	// 		
	// 		case (UseSlot.Secondary, InputPhase.Pressed):
	// 			IsAiming = true;
	// 			_chargeTime = 0.0f;
	// 			break;
	// 		
	// 		case (UseSlot.Secondary, InputPhase.Held):
	// 			if (IsAiming)
	// 			{
	// 				_chargeTime = MathF.Min(_chargeTime + (float)delta, MaxChargeTime);
	// 			}
	//
	// 			break;
	// 		
	// 		case (UseSlot.Secondary, InputPhase.Released):
	// 			IsAiming = false;
	// 			_chargeTime = 0.0f;
	// 			break;
	// 	};
	// }

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
