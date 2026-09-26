using Godot;
using Godot.Collections;
using upgradedspoon.globals;
using upgradedspoon.player.interaction.states;
using upgradedspoon.state;
using upgradedspoon.traits;

namespace upgradedspoon.player.interaction;

public partial class Interactor : Node3D
{
	[ExportGroup("Node References")]
	[Export] public RayCast3D PickupRaycast;
	[Export] public RayCast3D AimRaycast;
	[Export] public Camera3D Camera;
	[Export] public Node3D HoldPivot;
	[Export] public InteractorStateMachine StateMachine;

	[ExportGroup("Timing")] 
	[Export] public double PickupCooldown = 0.25f;
	
	public Holdable ObjectHeld;

	private Interactable _currentTarget;
	private Interactable _newTarget;

	public override void _Ready()
	{
		base._Ready();
		
		PickupRaycast.AddException(Owner.GetNode("CollisionShape3D") as CollisionObject3D);
		
		StateMachine.AddState("Idle", new InteractorIdleState());
		StateMachine.AddState("Holding", new InteractorHoldingState());
		
		StateMachine.Enter("Idle");
	}

	public override void _PhysicsProcess(double delta)
	{
		_newTarget = null;

		if (PickupRaycast.IsColliding())
		{
			var body = PickupRaycast.GetCollider();
			if (body is RigidBody3D node && node.IsInGroup("Interactable"))
			{
				_newTarget = GetInteractable(node);
			}
		}

		if (_newTarget != null && _currentTarget != null &&
			(_newTarget == _currentTarget || _newTarget == ObjectHeld))
		{
			return;
		}

		_currentTarget = _newTarget;

		if (_currentTarget != null)
		{
			if (ObjectHeld != null && _currentTarget is Holdable)
			{
				return;
			}
			
			_currentTarget.SetHighlight(true);
			SignalBus.Instance.RaiseInteractableSeen(_currentTarget);
			return;
		}
		SignalBus.Instance.RaiseLookedAway();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("interact") && _currentTarget != null)
		{
			_currentTarget.Interact(this, new Dictionary{["target"] = _currentTarget});
		}
	}

	private static Interactable GetInteractable(RigidBody3D body)
	{
		foreach (var child in body.GetChildren())
		{
			if (child is Interactable interactable)
			{
				return interactable;
			}
		}

		return null;
	}
}
