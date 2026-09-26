using System;
using Godot;
using Godot.Collections;
using upgradedspoon.state;
using upgradedspoon.traits;
using upgradedspoon.types;

namespace upgradedspoon.player.interaction.states;

public partial class InteractorHoldingState : State<Interactor>
{
#nullable enable
    private Holdable? _heldObject;

    public override void Enter(Dictionary? msg)
    {
        Actor.ObjectHeld = msg?["holdable"].As<Holdable>();

        if (Actor.ObjectHeld is null)
        {
            GD.PushWarning("HoldingState entered without a 'holdable' in message");
            OnTransitioned(this, "Idle", new Dictionary());
            return;
        }

        _heldObject = Actor.ObjectHeld;
        _heldObject.RaiseGrabbed();
        _heldObject.Released += OnReleased;
    }

    public override void Exit()
    {
        if (_heldObject is null) return;
        _heldObject.Released -= OnReleased;
        _heldObject = null;
    }

    public override void PhysicsUpdate(double delta)
    {
        _heldObject?.UpdateHold(Actor.HoldPivot, delta);

        if (Input.IsActionJustPressed("PrimaryAction")) _heldObject?.PrimaryPressed(GetAimContext());
        if (Input.IsActionPressed("PrimaryAction")) _heldObject?.PrimaryHeld(delta, GetAimContext());
        if (Input.IsActionJustReleased("PrimaryAction")) _heldObject?.PrimaryReleased(GetAimContext());


        if (Input.IsActionJustPressed("SecondaryAction")) _heldObject?.SecondaryPressed(GetAimContext());
        if (Input.IsActionPressed("SecondaryAction")) _heldObject?.SecondaryHeld(delta, GetAimContext());
        if (Input.IsActionJustReleased("SecondaryAction")) _heldObject?.SecondaryReleased(GetAimContext());
    }

    private AimContext GetAimContext()
    {
        if (_heldObject is null)
            return new AimContext(Vector3.Zero, 1.5f);
        
        var aimOrigin = Actor.Camera.GlobalPosition;
        var aimDirection = -Actor.Camera.GlobalBasis.Z;
        var aimPoint = aimOrigin + aimDirection * 1000.0f;

        if (Actor.AimRaycast.IsColliding() && Actor.AimRaycast.GetCollider() != null)
        {
            aimPoint = Actor.AimRaycast.GetCollisionPoint();
        }

        var direction = (aimPoint - _heldObject.Body.GlobalPosition).Normalized();
        return new AimContext(direction, 1.0f); // TODO - Replace with actor's strength mult
    }

    // Event Handlers
    private void OnReleased(object? sender, EventArgs e)
    {
        _heldObject?.SetShouldHover(false);
        if (_heldObject != null) _heldObject.IsAiming = true;
        OnTransitioned(this, "Idle", new Dictionary());
    }
}