class_name InteractorHoldingState
extends State

## Interactor is holding a Holdable: follow it to the hold pivot each physics
## tick, and forward primary/secondary input to it.

var _object_held: Holdable = null

func enter(message: Dictionary = {}) -> void:
	_object_held = message.get("holdable")
	if not _object_held:
		push_warning("HoldingState entered without a 'holdable' in message")
		transitioned.emit(self, &"Idle", {})
		return

	_object_held.grabbed.emit()
	_object_held.released.connect(_on_released)

func exit() -> void:
	if _object_held and _object_held.released.is_connected(_on_released):
		_object_held.released.disconnect(_on_released)

func physics_update(delta: float) -> void:
	_object_held.update_hold(actor.hold_pivot, delta)

	if Input.is_action_just_pressed("primary_action"):
		_object_held.primary_pressed(get_aim_context)
	if Input.is_action_pressed("primary_action"):
		_object_held.primary_held(delta, get_aim_context)
	if Input.is_action_just_released("primary_action"):
		_object_held.primary_released(get_aim_context)

	if Input.is_action_just_pressed("secondary_action"):
		_object_held.secondary_pressed(get_aim_context)
	if Input.is_action_pressed("secondary_action"):
		_object_held.secondary_held(delta, get_aim_context)
	if Input.is_action_just_released("secondary_action"):
		_object_held.secondary_released(get_aim_context)

func handle_input(event: InputEvent) -> void:
	if event.is_action_pressed("let_go"):
		_object_held.released.emit()

func get_aim_context() -> Dictionary:
	var aim_origin = actor.camera.global_position
	var aim_dir = -actor.camera.global_basis.z
	var aim_point = aim_origin + aim_dir * 1000.0

	if actor.aim_raycast.is_colliding() and actor.aim_raycast.get_collider() != _object_held.body:
		aim_point = actor.aim_raycast.get_collision_point()

	var direction = (aim_point - _object_held.body.global_position).normalized()
	return {
		"direction": direction,
		"strength_mult": actor.player_stats.strength_multiplier
	}

func _on_released() -> void:
	_object_held.set_should_hover(false)
	_object_held.is_aiming = false
	transitioned.emit(self, &"Idle", {"start_cooldown": true})
