class_name ShootableIdle
extends ShootableState

func enter(_msg: Dictionary = {}) -> void:
	if _msg.has("next_shot_queued") and _msg.get("next_shot_queued", true):
		transitioned.emit(self, &"Firing", {})

func primary_pressed(_aim_context: Callable) -> void:
	transitioned.emit(self, &"Firing", {})
