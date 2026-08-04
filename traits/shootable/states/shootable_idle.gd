class_name ShootableIdle
extends ShootableState

func primary_pressed(_aim_context: Callable) -> void:
	transitioned.emit(self, &"Firing", {})
