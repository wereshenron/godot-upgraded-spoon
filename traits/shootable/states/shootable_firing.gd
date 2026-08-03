extends ShootableState
class_name ShootableFiring

var time_since_shot: float = 0.0

func enter(_msg: Dictionary = {}) -> void:
	time_since_shot = 0.0
	actor.shoot()
	
func physics_update(_delta: float) -> void:
	time_since_shot += _delta
	if (time_since_shot >= actor.fire_rate):
		transitioned.emit(self, &"Idle", {})
