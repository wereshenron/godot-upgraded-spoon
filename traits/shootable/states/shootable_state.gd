class_name ShootableState
extends State

func primary_pressed(_aim_context: Callable) -> void: pass
func primary_held(_delta: float, _aim_context: Callable) -> void: pass
func primary_released(_aim_context: Callable) -> void: pass
func secondary_pressed(_aim_context: Callable) -> void: pass
func secondary_held(_delta: float, _aim_context: Callable) -> void: pass
func secondary_released(_aim_context: Callable) -> void: pass

func get_aim_blend_target() -> float:
	return 0.0
