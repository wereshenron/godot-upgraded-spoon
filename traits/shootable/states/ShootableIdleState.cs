using System;
using Godot.Collections;
using upgradedspoon.state;

namespace upgradedspoon.traits.shootable.states;

public partial class ShootableIdleState : State<Shootable>
{
	public override void Enter(Dictionary msg)
	{
		if (msg?.ContainsKey("nextShotQueued") == true && msg.TryGetValue("nextShotQueued", out _))
		{
			OnTransitioned(this, "Firing", new Dictionary());
		}
	}
}
