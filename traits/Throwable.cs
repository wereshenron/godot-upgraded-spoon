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
	[Export(PropertyHint.Range,"-180.0, 180.0, 1.0, degrees")]
	public float SpinTwistDegrees;

	[Export] public Vector3 ThrowOffset;
	[Export] public float MaxChargeTime = 1.5f;
	
	private float _chargeTime = 0.0f;

	public override Vector3 GetHoldOffset()
	{
		return HoldOffset.Lerp(ThrowOffset, IsAiming ? 1.0f : 0.0f);
	}

	public override double GetFollowSpeed(double baseSpeed)
	{
		return IsAiming ? AimFollowSpeed : baseSpeed;
	}

	public override void PrimaryPressed(AimContext? aimContext = null)
	{
		if (IsAiming)
		{
			var context = aimContext;
		}
	}
}
