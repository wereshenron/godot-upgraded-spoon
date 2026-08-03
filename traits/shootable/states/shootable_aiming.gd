extends ShootableState
class_name ShootableAiming
## Some type of shootable object is ADS: 
	# Move the shootable to ADS within some amount of time 
	
func enter(_msg: Dictionary = {}) -> void:
	SignalBus.show_reticle.emit(false)

func secondary_released(_aim_context: Callable) -> void:
	transitioned.emit(self, &"Idle", {})

func get_aim_blend_target() -> float:
	return 1.0
