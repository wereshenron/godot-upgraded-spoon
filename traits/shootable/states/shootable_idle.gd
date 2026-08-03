class_name ShootableIdle
extends ShootableState

#func enter(_msg: Dictionary = {}) -> void:
	#SignalBus.show_reticle.emit(true)

func primary_pressed(_aim_context: Callable) -> void:
	transitioned.emit(self, &"Firing", {})

#func secondary_pressed(_aim_context: Callable) -> void:
	#transitioned.emit(self, &"Aiming", {})
