using Godot.Collections;
using upgradedspoon.types;

namespace upgradedspoon.traits.shootable.states;

public partial class ShootableIdleState : ShootableState
{
    private bool _fireNextTick;

#nullable enable
    public override void Enter(Dictionary? msg)
    {
        _fireNextTick = msg is not null && msg.ContainsKey("nextShotQueued");
    }

    public override void PhysicsUpdate(double delta)
    {
        if (!_fireNextTick) return;
        _fireNextTick = false;
        TryFire();
    }

    public override void OnAction(UseSlot slot, InputPhase phase, double delta, AimContext ctx)
    {
        if (phase != InputPhase.Pressed) return;

        switch (slot)
        {
            case UseSlot.Primary: TryFire(); break;
            case UseSlot.Reload: TryReload(); break;
        }
    }

    public override void Cancelled() => _fireNextTick = false;

    private void TryFire()
    {
        if (Actor.Ammo > 0)
            OnTransitioned(this, "Firing", new Dictionary());
        else
            TryReload(); // empty mag: auto-reload (swap for a dry-fire click if you prefer)
    }

    private void TryReload()
    {
        if (Actor.CanReload)
            OnTransitioned(this, "Reload", new Dictionary());
    }
}