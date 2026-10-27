using System;
using Godot;
using Godot.Collections;
using upgradedspoon.player.camera;
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

    private Camera _camera;
    private AimContext _aimContext;
    private Vector3 _origin;
    private Vector3 _cameraForward;
    private Vector3 _aimPoint;
    private bool _contextBuilt;

#nullable enable
    private Holdable? _heldObject;

    public override void Enter(Dictionary? msg)
    {
        // // Initialization
        // _camera = Actor.Camera;
        // _origin = _camera.GlobalPosition;
        // _cameraForward = -_camera.GlobalBasis.Z;
        // _aimPoint = _origin + _cameraForward * 1000.0f;
        _aimContext = default!;
        
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

        _contextBuilt = false;
        _aimContext = default!;

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

            if (!_contextBuilt)
            {
                _aimContext = BuildAimContext(held); 
                _contextBuilt = true;
            }

            held.OnAction(slot, phase, 0, _aimContext);
        }
    }

    // Only continuous "held" polling lives here.
    public override void PhysicsUpdate(double delta)
    {
        // using var _ = GcWatcher.Track("InteractorHoldingState._PhysicsProcess");
        if (!TryGetHeld(out var held))
        {
            OnTransitioned(this, "Idle", new Dictionary());
            return;
        }

        held.UpdateHold(Actor.HoldPivot, delta);

        _contextBuilt = false;
        _aimContext = default!;

        foreach (var (action, slot) in Bindings)
        {
            if (!Input.IsActionPressed(action)) continue;

            if (!_contextBuilt) { _aimContext = BuildAimContext(held); _contextBuilt = true; }

            held.OnAction(slot, InputPhase.Held, delta, _aimContext);

            if (!TryGetHeld(out held)) return;
        }
    }

    public override void Update(double delta)
    {
        // using var _ = GcWatcher.Track("InteractorHoldingState.Update");
        _camera = Actor.Camera;
        _origin = _camera.GlobalPosition;
        _cameraForward = -_camera.GlobalBasis.Z;
        _aimPoint = _origin + _cameraForward * 1000.0f;
    }

    private bool TryGetHeld(out Holdable held)
    {
        held = _heldObject!;
        return _heldObject is not null && IsInstanceValid(_heldObject);
    }

    private AimContext BuildAimContext(Holdable held)
    {
        if (_camera.AimRaycast.IsColliding())
            _aimPoint = _camera.AimRaycast.GetCollisionPoint();

        return new AimContext(_origin, _cameraForward, _aimPoint);
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