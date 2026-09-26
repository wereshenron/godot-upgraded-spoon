using Godot;
using Godot.Collections;

namespace upgradedspoon.models.bullets;

public partial class Bullet : Node3D
{
	private ShaderMaterial _tracerMaterial;

	public Vector3 Velocity;
	public double GravityScale;
	public float Damage;
	public float Lifetime;
	private double _age;

	public override void _PhysicsProcess(double delta)
	{
		_age += delta;
		if (_age > Lifetime)
		{
			QueueFree();
			return;
		}

		var start = GlobalPosition;
		Velocity += Vector3.Down * (float) GravityScale * (float)delta;
		var end = start + Velocity * (float)delta;
		
		DrawTracer(start, end);

		var spaceState = GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(start, end);
		// query.Exclude = [this.];
		var result = spaceState.IntersectRay(query);

		if (result != null)
		{
			var position = result["position"].AsVector3();
			SetGlobalPosition(position);
			OnHit(result);
			QueueFree();
			return;
		}

		SetGlobalPosition(end);
	}

	private void OnHit(Dictionary result)
	{
		var collider = result["collider"].AsGodotObject();
		var hitPosition = result["position"].AsVector3();
		var impulse = Velocity.Normalized() * Damage;
		var offset = hitPosition - ((Node3D)collider).GlobalPosition;

		switch (collider)
		{
			case RigidBody3D rb:
				rb.ApplyImpulse(impulse, offset);
				return;
			case PhysicalBone3D bone:
				if (bone.GetParent() is PhysicalBoneSimulator3D skeleton)
				{
					// skeleton.PhysicalBonesStartSimulation([bone.Get("bone_name")]);
					PhysicsServer3D.BodyApplyImpulse(bone.GetRid(), impulse, offset);
					skeleton.PhysicalBonesStartSimulation();
				}
				break;
		}
	}

	private void DrawTracer(Vector3 start, Vector3 end)
	{
		var tracerMesh = new ImmediateMesh();
		tracerMesh.SurfaceBegin(Mesh.PrimitiveType.Lines, _tracerMaterial);
		tracerMesh.SurfaceSetColor(new Color(1, 1, 1));
		tracerMesh.SurfaceSetUV(new Vector2(0, 0));
		tracerMesh.SurfaceAddVertex(start);
		tracerMesh.SurfaceSetColor(new Color(1, 1, 1, 0));
		tracerMesh.SurfaceSetUV(new Vector2(1, 0));
		tracerMesh.SurfaceAddVertex(end);
		tracerMesh.SurfaceEnd();

		var tracerInstance = new MeshInstance3D();
		tracerInstance.Mesh = tracerMesh;
		tracerInstance.MaterialOverride = _tracerMaterial;
		GetTree().Root.AddChild(tracerInstance);

		var tween = tracerInstance.CreateTween();
		tween.TweenMethod(Callable.From((double progress) => ((ShaderMaterial)tracerInstance.MaterialOverride).SetShaderParameter("progress", progress)), 0.5f,
			1.0f, .22f);
		tween.TweenCallback(Callable.From(tracerInstance.QueueFree));
	}
}
