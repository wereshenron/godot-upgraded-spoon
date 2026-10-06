using System;
using Godot;
using upgradedspoon.traits;

namespace upgradedspoon.globals;

public class RecoilKickedArgs(Vector3 amount, Vector3 maxOffset) : EventArgs
{
	public Vector3 Amount { get; } = amount;
	public Vector3 MaxOffset { get; } = maxOffset;
}

public partial class SignalBus : Node
{
	public static SignalBus Instance { get; private set; }

	public event EventHandler LookedAway;
	public event EventHandler<bool> ShowReticle;
	public event EventHandler<RecoilKickedArgs> RecoilKicked;
	public event EventHandler<Interactable> InteractableSeen;

	public override void _Ready()
	{
		Instance = this;
	}

	public void RaiseLookedAway()
	{
		LookedAway?.Invoke(this, EventArgs.Empty);
	}

	public void RaiseShowReticle(bool visible)
	{
		ShowReticle?.Invoke(this, visible);
	}

	public void RaiseRecoilKicked(Vector3 origin, Vector3 direction)
	{
		RecoilKickedArgs args = new(origin, direction);
		RecoilKicked?.Invoke(this, args);
	}

	public void RaiseInteractableSeen(Interactable interactable)
	{
		InteractableSeen?.Invoke(this, interactable);
	}
}
