using System;
using Godot;
using upgradedspoon.player.interaction;
using upgradedspoon.traits;
using upgradedspoon.types;
using Interactable = upgradedspoon.traits.interactable.Interactable;

namespace upgradedspoon.globals;

public class RecoilKickedArgs(Vector3 amount, Vector3 maxOffset) : EventArgs
{
    public Vector3 Amount { get; } = amount;
    public Vector3 MaxOffset { get; } = maxOffset;
}

public class InteractableSeenArgs(Interactor sender, Interactable interactable) : EventArgs
{
    public Interactor Interactor { get; } = sender;
    public Interactable Interactable { get; } = interactable;
}

public partial class SignalBus : Node
{
    public static SignalBus Instance { get; private set; }

    public event EventHandler LookedAway;
    public event EventHandler<bool> ShowReticle;
    public event EventHandler<RecoilKickedArgs> RecoilKicked;
    public event EventHandler<InteractableSeenArgs> InteractableSeen;

    public event EventHandler<bool> AmmoShown;
    public event EventHandler<AmmoState> AmmoChanged;
    public event EventHandler<bool> PauseStarted;

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

    public void RaiseInteractableSeen(Interactor interactor, Interactable interactable)
    {
        InteractableSeenArgs args = new(interactor, interactable);
        InteractableSeen?.Invoke(this, args);
    }

    public void RaiseAmmoShown(object sender, bool e)
    {
        AmmoShown?.Invoke(sender, e);
    }

    public void RaiseAmmoChanged(AmmoState e)
    {
        AmmoChanged?.Invoke(this, e);
    }

    public void RaisePauseStarted(object sender, bool e)
    {
        PauseStarted?.Invoke(sender, e);
    }
}