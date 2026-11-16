using upgradedspoon.state;
using upgradedspoon.types;

namespace upgradedspoon.traits.shootable.states;

public abstract partial class ShootableState : State<Shootable>
{
    /// Single entry point for all gun input. States switch on (slot, phase) and ignore the rest.
    public virtual void OnAction(UseSlot slot, InputPhase phase, double delta, AimContext ctx) { }

    /// Gun was dropped or taken mid-input. Abort anything in progress.
    public virtual void Cancelled() { }
}