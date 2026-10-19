using System;
using Godot;
using Godot.Collections;
using upgradedspoon.globals;
using upgradedspoon.state;
using upgradedspoon.traits;
using upgradedspoon.types;

namespace upgradedspoon.player.interaction.states;

public partial class InteractorHoldingState : State<Interactor>
{
    private static readonly (StringName Action, UseSlot Slot)[] Bindings =
    [
        ("PrimaryAction", UseSlot.Primary),
        ("SecondaryAction", UseSlot.Secondary),
        ("ReloadAction", UseSlot.Reload)
    ];

#nullable enable
    private Holdable? _heldObject;

    public override void Enter(Dictionary? msg)
    {
        Holdable? holdable = null;
        if (msg is not null && msg.TryGetValue("holdable", out var value))
            holdable = value.As<Holdable>();

        if (holdable is null)
        {
            GD.PushWarning("HoldingState entered without a 'holdable' in message");
            OnTransitioned(this, "Idle", new Dictionary());
            return;
        }

        holdable.Body.Freeze = true;
        _heldObject = holdable;
        Actor.ObjectHeld = holdable;
        holdable.Released += OnReleased;
        holdable.RaiseGrabbed();
    }

    public override void Exit()
    {
        if (_heldObject is not null && IsInstanceValid(_heldObject))
            _heldObject.Released -= OnReleased;

        Actor.ObjectHeld.Body.Freeze = false;
        _heldObject = null;
        Actor.ObjectHeld = null;
    }

    // Discrete presses/releases arrive as events, so a quick tap can't be missed between ticks.
    public override void HandleInput(InputEvent @event)
    {
        if (!TryGetHeld(out var held)) return;
        if (!@event.IsActionType()) return;

        var built = false;
        AimContext ctx = default!;

        if (@event.IsActionReleased("LetGo"))
        {
            _heldObject?.RaiseReleased();
            return;
        }

        foreach (var (action, slot) in Bindings)
        {
            if (!@event.IsAction(action)) continue;
            if (@event.IsEcho()) continue;

            var phase = @event.IsPressed() ? InputPhase.Pressed : InputPhase.Released;

            if (!built) { ctx = BuildAimContext(held); built = true; }

            held.OnAction(slot, phase, 0, ctx);
        }
    }

    // Only continuous "held" polling lives here.
    public override void PhysicsUpdate(double delta)
    {
        if (!TryGetHeld(out var held))
        {
            OnTransitioned(this, "Idle", new Dictionary());
            return;
        }

        held.UpdateHold(Actor.HoldPivot, delta);

        var built = false;
        AimContext ctx = default!;

        foreach (var (action, slot) in Bindings)
        {
            if (!Input.IsActionPressed(action)) continue;

            if (!built) { ctx = BuildAimContext(held); built = true; }

            held.OnAction(slot, InputPhase.Held, delta, ctx);

            if (!TryGetHeld(out held)) return;
        }
    }

    private bool TryGetHeld(out Holdable held)
    {
        held = _heldObject!;
        return _heldObject is not null && IsInstanceValid(_heldObject);
    }

    private AimContext BuildAimContext(Holdable held)
    {
        var camera = Actor.Camera;
        var origin = camera.GlobalPosition;
        var cameraForward = -camera.GlobalBasis.Z;
        var aimPoint = origin + cameraForward * 1000.0f;
        
        if (camera.AimRaycast.IsColliding())
            aimPoint = camera.AimRaycast.GetCollisionPoint();

        return new AimContext(origin, cameraForward, aimPoint);
    }

    private void OnReleased(object? sender, EventArgs e)
    {
        if (_heldObject is not null)
        {
            _heldObject.SetShouldHover(false);
            _heldObject.IsAiming = false;
        }

        OnTransitioned(this, "Idle", new Dictionary());
    }
}