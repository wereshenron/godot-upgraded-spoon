class_name Shootable
extends Holdable

@export var aim_offset: Vector3
@export var aim_transition_time: float = 0.15  # seconds to fully reach aim_offset

var aim_blend: float = 0.0  # 0 = hold_offset, 1 = aim_offset

func get_hold_offset() -> Vector3:
	return hold_offset.lerp(aim_offset, aim_blend)

func get_follow_speed(base_speed: float) -> float:
	return aim_follow_speed if is_aiming else base_speed

@warning_ignore("standalone_expression")
func secondary_pressed(_aim_context: Callable = func(): null) -> void:
	is_aiming = true

@warning_ignore("standalone_expression")
func secondary_released(_aim_context: Callable = func(): null) -> void:
	is_aiming = false

func update_hold(hold_pivot: Node3D, delta: float) -> void:
	if !hold_pivot: return

	# advance the aim transition
	var target_blend: float = 1.0 if is_aiming else 0.0
	aim_blend = move_toward(aim_blend, target_blend, delta / aim_transition_time)

	var forward = -hold_pivot.global_basis.z
	var right = hold_pivot.global_basis.x
	var up = hold_pivot.global_basis.y
	var offset = get_hold_offset()  # now uses aim_blend, not a hard 0/1

	var hold_target: Vector3 = hold_pivot.global_position \
		+ forward * offset.z \
		+ right * offset.x \
		+ up * offset.y

	var mass: float = 1.0
	var object_mass = body.get("mass")
	if object_mass != null:
		mass = object_mass

	var speed = get_follow_speed(base_follow_speed)
	var follow_speed: float = speed / (1.0 + mass * mass_influence)

	body.global_position = body.global_position.lerp(
		hold_target,
		clampf(follow_speed * delta, 0.0, 1.0)
	)
