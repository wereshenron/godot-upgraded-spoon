#nullable enable
using Godot;
using Godot.Collections;

namespace upgradedspoon.traits.shootable.states;

public partial class ShootableReloadingState : ShootableState
{

	public override void Enter(Dictionary? msg)
	{
		Actor.AnimationPlayer.CurrentAnimationChanged += OnAnimationEnded;
		Actor.AnimationPlayer.Play("Reload");
	}
	
	public override void Exit()
	{
		Actor.AnimationPlayer.CurrentAnimationChanged -= OnAnimationEnded;
	}

	private void OnAnimationEnded(StringName animName)
	{
		StringName empty = "";
		if (animName != empty) return;
		
		Actor.RefillMagazine();
		OnTransitioned(this, Actor.IdleState,new Dictionary());
	}
}
