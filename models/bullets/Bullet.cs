using System;
using Godot;
using Godot.Collections;

namespace upgradedspoon.models.bullets;

public partial class Bullet : Area3D
{
	[Export] public float TimeToLive = 3f;
	
	public Vector3 Velocity;
	public float Mass; 
	public Vector3 CollisionPoint;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	public override void _Process(double delta)
	{
		if (TimeToLive <= 0f)
		{
			QueueFree();
		}

		TimeToLive -= (float)delta;
	}

	public override void _PhysicsProcess(double delta)
	{
		Position += Velocity * (float)delta;
	}

	public override void _ExitTree()
	{
		BodyEntered -= OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body is not RigidBody3D rb) return;
		GD.Print("Should be hitting?");

		rb.ApplyImpulse(Velocity * Mass, CollisionPoint - rb.GlobalPosition);
		// rb.ApplyCentralImpulse(Velocity * Mass);
		QueueFree();
	}
}
