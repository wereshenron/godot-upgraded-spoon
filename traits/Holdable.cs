using System;
using Godot;
using Godot.Collections;
using upgradedspoon.globals;
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
	[Export] public RigidBody3D Body;

	public virtual Vector3 GetHoldOffset() => Vector3.Zero;
	public virtual double GetFollowSpeed(double baseSpeed) => baseSpeed;

	public virtual bool CanUse() => true;

	public virtual void UpdateHold(Node3D holdPivot, double delta)
	{
	}

	public bool IsAiming { get; set; }

	public event EventHandler Grabbed;
	public event EventHandler Released;

	protected double Movement;
	protected bool HasSpiked;

	protected internal void RaiseGrabbed() => Grabbed?.Invoke(this, EventArgs.Empty);
	protected internal void RaiseReleased() => Released?.Invoke(this, EventArgs.Empty);
	
	private bool _isMonitored = false;

	public override void _Ready()
	{
		base._Ready();

		Body?.AddToGroup("Holdable");

		Grabbed += OnGrabbed;

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
		if (Body == null) return;
		HandleCcd();
	}

	private void OnGrabbed(object sender, EventArgs args)
	{
		SetHighlight(false);
		SignalBus.Instance.RaiseLookedAway();

		_isMonitored = true;
	}

	// Helpers
	private void HandleCcd()
	{
		if (!Body.ContinuousCd && !HasSpiked)
		{
			return;
		}

		Movement = Mathf.Clamp(Body.LinearVelocity.Length(), 0.0, 1.0);

		// This is the first instance of seeing quick movement - set CCD = true and continue 
		if (Movement > MovementLowerThreshold && !Body.ContinuousCd)
		{
			HasSpiked = true;
			Body.ContinuousCd = true;
			return;
		}
		
		// Return if there is quick movement, CCD is true, and it's spiked
		if (Movement > MovementLowerThreshold && Body.ContinuousCd)
		{
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
	public virtual void OnAction(UseSlot slot, InputPhase phase, double delta, AimContext ctx)
	{
	}
}
