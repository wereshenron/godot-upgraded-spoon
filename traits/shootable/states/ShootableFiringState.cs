using Godot.Collections;
using upgradedspoon.types;

namespace upgradedspoon.traits.shootable.states;

public partial class ShootableFiringState : ShootableState
{
    private bool _nextShotQueued;
    private double _timeSinceShot;

#nullable enable
    public override void Enter(Dictionary? msg)
    {
        _timeSinceShot = 0.0;
        _nextShotQueued = false;
        Actor.Shoot();
    }

    public override void PhysicsUpdate(double delta)
    {
        _timeSinceShot += delta;
        if (_timeSinceShot < Actor.GunSettings.FireRate) return;

        var msg = _nextShotQueued
            ? new Dictionary { ["nextShotQueued"] = true }
            : new Dictionary();
        OnTransitioned(this, "Idle", msg);
    }

    public override void OnAction(UseSlot slot, InputPhase phase, double delta, AimContext ctx)
    {
        if (slot != UseSlot.Primary) return;

        _nextShotQueued = phase switch
        {
            InputPhase.Held => true,
            InputPhase.Released => false,
            _ => _nextShotQueued
        };
    }

    public override void Cancelled() => _nextShotQueued = false;
}