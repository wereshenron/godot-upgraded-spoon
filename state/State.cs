using System;
using Godot;
using Godot.Collections;
using upgradedspoon.types;

namespace upgradedspoon.state;

public abstract partial class State<TActor> : Node where TActor : Node3D
{
    //# Base class for a single state in a StateMachine. States are plain objects,

    public event Action<State<TActor>, StringName, Dictionary> Transitioned;

    public TActor Actor { get; set; }

    // State Management

#nullable enable
    public virtual void Enter(Dictionary? msg)
    {
    }

    public virtual void Exit()
    {
    }

    public virtual void Update(double delta)
    {
    }

    public virtual void PhysicsUpdate(double delta)
    {
    }

    public virtual void HandleInput(InputEvent @event)
    {
    }

    // State-specific input delegation
    public virtual void PrimaryPressed(AimContext? aimContext = null)
    {
    }

    public virtual void PrimaryHeld(double delta, AimContext? aimContext = null)
    {
    }

    public virtual void PrimaryReleased(AimContext? aimContext = null)
    {
    }

    public virtual void SecondaryPressed(AimContext? aimContext = null)
    {
    }

    public virtual void SecondaryHeld(double delta, AimContext? aimContext = null)
    {
    }

    public virtual void SecondaryReleased(AimContext? aimContext = null)
    {
    }


    protected virtual void OnTransitioned(State<TActor> currentState, StringName currentStateId, Dictionary message)
    {
        Transitioned?.Invoke(currentState, currentStateId, message);
    }
}