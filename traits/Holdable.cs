using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using upgradedspoon.player.interaction;
using upgradedspoon.player.interaction.states;
using upgradedspoon.types;

namespace upgradedspoon.traits;

public abstract partial class Holdable : Interactable
{
	[Export] public Vector3 HoldOffset;
	[Export] public double MovementLowerThreshold;
	[Export] public double BaseFollowSpeed;
	[Export] public double AimFollowSpeed;
	[Export] public double MassInfluence;

	public virtual Vector3 GetHoldOffset() => Vector3.Zero;
	public virtual double GetFollowSpeed(double baseSpeed) => baseSpeed;

	public virtual bool CanUse() => true;

	public virtual void UpdateHold(Node3D holdPivot, double delta)
	{
	}
	
	public bool IsAiming { get; set; }
	
	public event EventHandler Grabbed;
	public event EventHandler Released;

	protected internal RigidBody3D Body { get; set; }
	protected double Movement;
	protected bool HasSpiked;

	protected internal void RaiseGrabbed() => Grabbed?.Invoke(this, EventArgs.Empty);
	protected internal void RaiseReleased() => Released?.Invoke(this, EventArgs.Empty);

	public override void _Ready()
	{
		base._Ready();
		Body = GetParent() as RigidBody3D;
		Body?.AddToGroup("Holdable");

		Grabbed += (_, _) => OnGrabbed();

		Action = "Hold";
	}

	public override void Interact(Interactor interactor, Dictionary msg = null)
	{
		var target = msg?["target"].As<Interactable>();
		if (target == null || interactor.StateMachine.CurrentState is InteractorHoldingState)
			return;
		interactor.StateMachine.TransitionTo("Holding", new Dictionary { ["holdable"] = target });
	}

	public override void _PhysicsProcess(double delta)
	{
		HandleCcd();
	}

	private void OnGrabbed()
	{
		SetHighlight(false);
		SetShouldHover(false);
		globals.SignalBus.Instance.RaiseLookedAway();
	}

	// Helpers
	private void HandleCcd()
	{
		if (Body.ContinuousCd)
		{
			HasSpiked = false;
			return;
		}

		Movement = Mathf.Clamp(Body.LinearVelocity.Length(), 0.0, 1.0);

		if (Movement > MovementLowerThreshold)
		{
			HasSpiked = true;
			return;
		}

		Body.ContinuousCd = false;
		HasSpiked = false;
	}

#nullable enable
	public void SetShouldHover(bool shouldHover)
	{
		Body.AxisLockLinearY = shouldHover;
	}

	/// Semantic input from whoever is holding this. Override only the slots you care about.
	/// `delta` is 0 for Pressed/Released and the frame step for Held.
	public virtual void OnAction(UseSlot slot, InputPhase phase, double delta, AimContext ctx) { }
}
