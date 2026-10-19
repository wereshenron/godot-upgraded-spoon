using Godot;
using Godot.Collections;
using upgradedspoon.globals;
using upgradedspoon.player.interaction.states;
using upgradedspoon.player.stats;
using upgradedspoon.traits;
using Camera = upgradedspoon.player.camera.Camera;

namespace upgradedspoon.player.interaction;

public partial class Interactor : Node3D
{
	private static readonly StringName InteractAction = "interact";

	[ExportGroup("Node References")]
	[Export] public Node3D HoldPivot;
	[Export] public InteractorStateMachine StateMachine;
	[Export] public Camera Camera;
	public PlayerStats PlayerStats;

	[ExportGroup("Timing")]
	[Export] public double PickupCooldown = 0.25f;

	public Holdable ObjectHeld;

	private Interactable _currentTarget;

	public override void _Ready()
	{
		base._Ready();

		// Assumes Owner is the player body. The old code cast a CollisionShape3D, which is
		// not a CollisionObject3D, so it was always null.
		// if (Owner is CollisionObject3D body)
		// 	Camera.PickupRaycast.AddException(body);

		StateMachine.AddState("Idle", new InteractorIdleState());
		StateMachine.AddState("Holding", new InteractorHoldingState());
		StateMachine.Enter("Idle");
	}

	public override void _PhysicsProcess(double delta) => SetTarget(FindTarget());

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed(InteractAction) && _currentTarget is not null)
			_currentTarget.Interact(this, new Dictionary { ["target"] = _currentTarget });
	}

	private Interactable FindTarget()
	{
		if (!Camera.PickupRaycast.IsColliding()) return null;
		if (Camera.PickupRaycast.GetCollider() is not RigidBody3D body || !body.IsInGroup("Interactable")) return null;

		var target = GetInteractable(body);
		if (target is null || target == ObjectHeld) return null;
		if (ObjectHeld is not null && target is Holdable) return null; // hands full
		return target;
	}

	private void SetTarget(Interactable next)
	{
		if (_currentTarget is not null && !IsInstanceValid(_currentTarget))
		{
			_currentTarget = null;
			SignalBus.Instance.RaiseLookedAway();
		}

		if (next == _currentTarget) return;

		_currentTarget?.SetHighlight(false);
		_currentTarget = next;

		if (next is null)
		{
			SignalBus.Instance.RaiseLookedAway();
			return;
		}

		next.SetHighlight(true);
		SignalBus.Instance.RaiseInteractableSeen(next);
	}

	private static Interactable GetInteractable(RigidBody3D body)
	{
		foreach (var child in body.GetChildren())
			if (child is Interactable interactable)
				return interactable;
		return null;
	}
}
