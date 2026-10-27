using System;
using System.Threading.Tasks;
using Godot;
using upgradedspoon.playground.ragdoll;

namespace upgradedspoon.models.bullets;

public partial class Bullet : Node3D
{
	[Export] public float TimeToLive = 3f;
	[Export(PropertyHint.Layers3DPhysics)] public uint CollisionMask = 1;

	public Vector3 Velocity;
	public float Mass;

	public event EventHandler BulletHit;
	

	public override void _PhysicsProcess(double delta)
	{
		TimeToLive -= (float)delta;
		if (TimeToLive <= 0f)
		{
			QueueFree();
			return;
		}

		MoveBullet((float)delta);
	}
	
	private void MoveBullet(float delta)
	{
		Vector3 from = GlobalPosition;
		Vector3 to = from + Velocity * (float)delta;

		var space = GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(from, to, CollisionMask);
		// query.CollideWithAreas = false; // default is false already
		// query.Exclude = new Godot.Collections.Array<Rid> { shooterRid };

		var hit = space.IntersectRay(query);

		if (hit.Count > 0)
		{
			var point = (Vector3)hit["position"];
			var collider = hit["collider"].As<Node>();

			switch (collider)
			{
				case RigidBody3D rb:
					rb.ApplyImpulse(Velocity * Mass, point - rb.GlobalPosition);
					break;
				case PhysicalBone3D pb:
					var receiver = pb.Owner?.GetNodeOrNull<RagdollReceiver>("RagdollReceiver");
					receiver?.Hit(pb, Velocity * Mass, point);
					break;
			}

			QueueFree();
			return;
		}

		GlobalPosition = to;
	}
	
	public class BulletHitArgs(Vector3 hitPoint, Vector3 direction) : EventArgs
	{
		public Vector3 HitPoint { get; } = hitPoint;
		public Vector3 Direction { get; } = direction;
	}
}
