extends ShootableState
class_name ShootableFiring

var time_since_shot: float = 0.0
var next_shot_queued: bool = false

func enter(_msg: Dictionary = {}) -> void:
	time_since_shot = 0.0
	actor.shoot()
	
func physics_update(_delta: float) -> void:
	time_since_shot += _delta
	if (time_since_shot >= actor.fire_rate):
		if !next_shot_queued:
			transitioned.emit(self, &"Idle", {})
		else:
			transitioned.emit(self, &"Idle", {"next_shot_queued": true})
		
func primary_held(_delta: float, _aim_context: Callable) -> void:
	if !actor.full_auto:
		return
	next_shot_queued = true
		
func primary_released(_aim_context: Callable) -> void:
	next_shot_queued = false
