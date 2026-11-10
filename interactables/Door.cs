using Godot;
using Godot.Collections;
using upgradedspoon.player.interaction;
using upgradedspoon.traits;

namespace upgradedspoon.interactables;

public partial class Door : Interactable
{
	public override void Interact(Interactor interactor, Dictionary msg = null)
	{
		GD.Print("Interacted");
	}
}
