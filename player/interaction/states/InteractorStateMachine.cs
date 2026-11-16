using Godot;
using upgradedspoon.state;

namespace upgradedspoon.player.interaction.states;

public partial class InteractorStateMachine : StateMachine<Interactor>
{
    [Export] public Interactor ActorNode
    {
        get => Actor;
        set => Actor = value;
    }
}