using Godot;
using Godot.Collections;

namespace upgradedspoon.state;

public partial class StateMachine<TActor> : Node where TActor : Node3D
{
	[Export] public NodePath ActorPath { get; set; }
	protected TActor Actor { get; set; }

	public override void _Ready()
	{
		Actor = GetNode<TActor>(ActorPath);
		if (Actor == null)
			GD.PushError($"{Name}: node at {ActorPath} is not a {typeof(TActor).Name}");
	}

	public State<TActor> CurrentState;
	public StringName CurrentStateId;
	protected readonly System.Collections.Generic.Dictionary<string, State<TActor>> States = new();

	public void AddState(StringName id, State<TActor> state)
	{
		state.Actor = Actor;
		state.Transitioned += _OnStateTransitioned;
		States[id] = state;
	}

	public void Enter(StringName stateId, Dictionary msg = null)
	{
		msg ??= new Dictionary();

		if (!States.ContainsKey(stateId))
		{
			GD.PushError($"StateMachine: no state registered with id '{stateId}'");
			return;
		}
		CurrentStateId = stateId;
		States.TryGetValue(stateId, out CurrentState);
		CurrentState?.Enter(msg);
	}

	public override void _Process(double delta)
	{
		
		CurrentState?.Update(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		CurrentState?.PhysicsUpdate(delta);
	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		CurrentState?.HandleInput(inputEvent);
	}
	
	public override void _ExitTree()
	{
		foreach (var state in States.Values)
		{
			state.Free();
		}
	}

	public void TransitionTo(StringName stateId, Dictionary msg = null)
	{

		if (!States.ContainsKey(stateId))
		{
			GD.PushWarning($"StateMachine: no state registered with id '{stateId}'");
			return;
		}

		CurrentState?.Exit();
		CurrentStateId = stateId;
		CurrentState = States[stateId];
		CurrentState.Enter(msg);
	}

	protected void _OnStateTransitioned(State<TActor> state, StringName newStateId, Dictionary msg)
	{
		if (state != CurrentState)
		{
			return;
		}
		// stale signal from a state that already exited
		TransitionTo(newStateId, msg);
	}
}
