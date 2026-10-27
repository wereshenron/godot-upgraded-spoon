using Godot;

namespace upgradedspoon.playground.ragdoll;

[GlobalClass]
public partial class ProceduralWalk : SkeletonModifier3D
{
	[Export] public StringName LeftThigh { get; set; } = "thigh.l";
	[Export] public StringName RightThigh { get; set; } = "thigh.r";
	[Export] public float SwingDegrees { get; set; } = 30f;
	[Export] public float SpeedScale { get; set; } = 6f;

	// Set by the enemy script each frame (0..1 normalized speed)
	[Export] public float MoveSpeed { get; set; } = 1f;

	private float _phase;

	// public override void _ProcessModificationWithDelta(double delta)
	// {
	// 	var skel = GetSkeleton();
	// 	if (skel == null) return;
	// 	
	// 	_phase += (float)delta * MoveSpeed * SpeedScale;
	// 	
	// 	var l = skel.FindBone(LeftThigh);
	// 	var r = skel.FindBone(RightThigh);
	// 	
	// 	if (l < 0 || r < 0) return;
	// 	
	// 	var swing = Mathf.DegToRad(SwingDegrees) * Mathf.Clamp(MoveSpeed, 0f, 1f);
	// 	var s = Mathf.Sin(_phase) * swing;
	// 	
	// 	var restL = skel.GetBoneRest(l).Basis.GetRotationQuaternion();
	// 	var restR = skel.GetBoneRest(r).Basis.GetRotationQuaternion();
	// 	
	// 	
	// 	skel.SetBonePoseRotation(l, restL * new Quaternion(Vector3.Back, s));
	// 	skel.SetBonePoseRotation(r, restR * new Quaternion(Vector3.Back, s));
	// }
}
