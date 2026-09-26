using Godot;
using upgradedspoon.state;

namespace upgradedspoon.traits.shootable.states;

public partial class ShootableStateMachine : StateMachine<Shootable>
{
    [Export] public Shootable ActorNode
    {
        get => Actor;
        set => Actor = value;
    }

    // [Export] public State<Shootable>[] ShootableStates;
}