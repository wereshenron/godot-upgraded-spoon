using System;
using Godot;
using upgradedspoon.types;
using static Godot.PropertyHint;

namespace upgradedspoon.traits;

public partial class Throwable : Holdable
{
	[Export] public double MinThrowForce;
	[Export] public double MaxThrowForce;
	[Export] public double AngularVelocityScale;

	[Export(PropertyHint.Range, "-180.0, 180.0, 1.0, degrees")]
	public float SpinTwistDegrees;

	[Export] public Vector3 ThrowOffset;
	[Export] public float MaxChargeTime = 1.5f;

	private float _chargeTime;

	public override Vector3 GetHoldOffset()
	{
		return HoldOffset.Lerp(ThrowOffset, IsAiming ? 1.0f : 0.0f);
	}

	public override double GetFollowSpeed(double baseSpeed)
	{
		return IsAiming ? AimFollowSpeed : baseSpeed;
	}

	public override void OnAction(UseSlot slot, InputPhase phase, double delta, AimContext ctx)
	{
		switch (slot, phase)
		{
			case (UseSlot.Primary, InputPhase.Pressed):
				if (IsAiming)
				{
					Throw(ctx.Direction, ctx.StrengthMultiplier);
				}

				IsAiming = false;
				_chargeTime = 0.0f;
				break;
			
			case (UseSlot.Secondary, InputPhase.Pressed):
				IsAiming = true;
				_chargeTime = 0.0f;
				break;
			
			case (UseSlot.Secondary, InputPhase.Held):
				if (IsAiming)
				{
					_chargeTime = MathF.Min(_chargeTime + (float)delta, MaxChargeTime);
				}

				break;
			
			case (UseSlot.Secondary, InputPhase.Released):
				IsAiming = false;
				_chargeTime = 0.0f;
				break;
		};
	}

	private void Throw(Vector3 direction, float strength = 1.0f)
	{
		var chargeRatio = _chargeTime / MathF.Max(MaxChargeTime, 0.0001f);
		var force = Mathf.Lerp(MinThrowForce, MaxThrowForce, chargeRatio) * strength;
		var relativeAngularVelocity = GetRelativeAngularVelocity(direction);
		var chargedAngularVelocity = Vector3.Zero.Lerp(relativeAngularVelocity, chargeRatio);

		RaiseReleased();
		Body.ContinuousCd = true;
		Body.AxisLockLinearY = false;
		Body.ApplyCentralImpulse(direction * (float)force);
		Body.AngularVelocity = chargedAngularVelocity;
	}

	private Vector3 GetRelativeAngularVelocity(Vector3 direction)
	{
		var dirNorm = direction.Normalized();
		var baseAxis = Vector3.Up.Cross(dirNorm).Normalized();
		var spinAxis = baseAxis.Rotated(dirNorm, Mathf.DegToRad(SpinTwistDegrees));
		return spinAxis * (float)AngularVelocityScale;
	}

	public override void UpdateHold(Node3D holdPivot, double delta)
	{
		if (holdPivot is null)
		{
			return;
		}

		var forward = -holdPivot.GlobalBasis.Z;
		var right = holdPivot.GlobalBasis.X;
		var up = -holdPivot.GlobalBasis.Y;
		var offset = GetHoldOffset();
		
		var holdTarget = holdPivot.GlobalPosition 
			+ forward * offset.Z
			+ right * offset.X
			+ up * offset.Y;

		var mass = Body.Mass;
		var speed = GetFollowSpeed(BaseFollowSpeed);
		var followSpeed = speed / (1.0 + mass * MassInfluence);

		Body.GlobalPosition = Body.GlobalPosition.Lerp(holdTarget, (float)Math.Clamp(followSpeed * delta, 0.0, 1.0));
	}
}
