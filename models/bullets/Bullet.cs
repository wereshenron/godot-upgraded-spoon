using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using upgradedspoon.playground.ragdoll;

namespace upgradedspoon.models.bullets;

public partial class Bullet : Node3D
{
	[Export] public float TimeToLive = 3f;
	[Export(PropertyHint.Layers3DPhysics)] public uint CollisionMask = 1;

	public Vector3 Velocity;
	public float Mass;

	public event EventHandler BulletHit;

	private PhysicsDirectSpaceState3D _space;
	private PhysicsRayQueryParameters3D _query;
	private Vector3 _from;
	private Vector3 _to;
	private Dictionary _hit;

	public override void _Ready()
	{
		_space = GetWorld3D().DirectSpaceState;
		_query = new PhysicsRayQueryParameters3D();
	}


	public override void _PhysicsProcess(double delta)
	{
		// using var _ = GcWatcher.Track("Bullet._Process");
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
		_from = GlobalPosition;
		_to = _from + Velocity * delta;

		_query.From = _from;
		_query.To = _to;
		_query.CollisionMask = CollisionMask;

		_hit = _space.IntersectRay(_query);

		if (_hit.Count > 0)
		{
			var point = (Vector3)_hit["position"];
			var collider = _hit["collider"].As<Node>();

			GD.Print(collider);

			switch (collider)
			{
				case RigidBody3D rb:
					rb.ApplyImpulse(Velocity * Mass, point - rb.GlobalPosition);
					break;
				case PhysicalBone3D pb:
					var receiver = pb.Owner?.GetNodeOrNull<RagdollReceiver>("RagdollReceiver");
					receiver?.Hit(pb, Velocity * Mass, point);
					break;
				case CharacterBody3D:
					return;
			}

			QueueFree();
			return;
		}

		GlobalPosition = _to;
	}
	
	public class BulletHitArgs(Vector3 hitPoint, Vector3 direction) : EventArgs
	{
		public Vector3 HitPoint { get; } = hitPoint;
		public Vector3 Direction { get; } = direction;
	}
}
