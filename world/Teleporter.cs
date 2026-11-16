using Godot;

namespace upgradedspoon.world;

public partial class Teleporter : Area3D
{
	[Export] public Marker3D SpawnPoint;
	[Export] public CollisionShape3D CollisionShape;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{
		switch (body)
		{
			case RigidBody3D rb:
				rb.Freeze = true;
				rb.GlobalPosition = SpawnPoint.GlobalPosition;
				rb.Freeze = false;
				break;
			case PhysicalBone3D pb:
				var sim = pb.GetParent<PhysicalBoneSimulator3D>();
				sim.PhysicalBonesStopSimulation();
				var root = sim.GetOwner<Node3D>();
				root.GlobalPosition = SpawnPoint.GlobalPosition;
				break;
			default:
				body.GlobalPosition = SpawnPoint.GlobalPosition;
				break;
		}
	}
}
