using System;
using Godot.Collections;
using upgradedspoon.state;
using upgradedspoon.types;

namespace upgradedspoon.traits.shootable.states;

public partial class ShootableFiringState : State<Shootable>
{
    private bool _nextShotQueued;
    private double _timeSinceShot;

#nullable enable
    public override void Enter(Dictionary? msg)
    {
        _timeSinceShot = 0.0;
        Actor.Shoot();
    }

    public override void PhysicsUpdate(double delta)
    {
        base.PhysicsUpdate(delta);
        _timeSinceShot += delta;
        if (!(_timeSinceShot >= Actor.GunSettings.FireRate)) return;
        if (!_nextShotQueued)
        {
            OnTransitioned(this, "Idle", new Dictionary());
            return;
        }
        
        OnTransitioned(this, "Idle", new Dictionary { ["nextShotQueued"] = "true" });
    }

    public override void PrimaryHeld(double delta, AimContext? aimContext = null)
    {
        if (!_nextShotQueued)
        {
            return;
        }

        _nextShotQueued = true;
    }

    public override void PrimaryReleased(AimContext? aimContext = null)
    {
        _nextShotQueued = false;
    }
}