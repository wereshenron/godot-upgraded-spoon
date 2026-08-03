class_name Shootable
extends Holdable

@export_category("Node References")
@export var state_machine: StateMachine

@export_category("Aiming")
@export var aim_offset: Vector3
@export var aim_transition_time: float = 0.15  # seconds to fully reach aim_offset

@onready var aim_raycast: RayCast3D = get_viewport().get_camera_3d().get_node("AimRaycast")

var aim_blend: float = 0.0  # 0 = hold_offset, 1 = aim_offset
var initial_layer
var initial_mask

func _ready() -> void:
	super._ready()
	
	# Initialize State
	state_machine.add_state(&"Idle", ShootableIdle.new())
	state_machine.add_state(&"Aiming", ShootableAiming.new())
	state_machine.start(&"Idle")
	initial_layer = body.collision_layer
	initial_mask = body.collision_mask
	
	# Set up signal handlers
	grabbed.connect(_on_grabbed)
	released.connect(_on_released)

func get_hold_offset() -> Vector3:
	return hold_offset.lerp(aim_offset, aim_blend)

func get_follow_speed(base_speed: float) -> float:
	if state_machine.current_state.get_aim_blend_target() > 0.0:
		return aim_follow_speed
	
	return base_speed

@warning_ignore("standalone_expression")
func secondary_pressed(_aim_context: Callable = func(): null) -> void:
	state_machine.current_state.secondary_pressed(_aim_context)

@warning_ignore("standalone_expression")
func secondary_held(_delta: float, _aim_context: Callable = func(): null) -> void:
	state_machine.current_state.secondary_held(_delta, _aim_context)

@warning_ignore("standalone_expression")
func secondary_released(_aim_context: Callable = func(): null) -> void:
	state_machine.current_state.secondary_released(_aim_context)

func update_hold(hold_pivot: Node3D, delta: float) -> void:
	if !hold_pivot: return

	# advance the aim transition
	var target_blend: float = state_machine.current_state.get_aim_blend_target()
	aim_blend = move_toward(aim_blend, target_blend, delta / aim_transition_time)

	var forward = -hold_pivot.global_basis.z
	var right = hold_pivot.global_basis.x
	var up = hold_pivot.global_basis.y
	var offset = get_hold_offset()  # now uses aim_blend, not a hard 0/1

	var hold_target: Vector3 = hold_pivot.global_position \
		+ forward * offset.z \
		+ right * offset.x \
		+ up * offset.y
	
	# Aiming
	var aim_origin = aim_raycast.global_position
	var aim_dir = -aim_raycast.global_basis.z
	var aim_point = aim_origin + aim_dir * 1000
	var direction = (aim_point - body.global_position).normalized()
	var target_basis = Basis.looking_at(direction, hold_pivot.global_basis.y)
	
	#hold_target = hold_target.lo
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
	body.global_basis = body.global_basis.slerp(
		target_basis,
		clampf(follow_speed * delta, 0.0, 1.0)
	)
	
	
### Signal Handlers ###
func _on_grabbed() -> void:
	body.freeze = true
	body.collision_layer = 0
	body.collision_mask = 0
	
func _on_released() -> void:
	state_machine.start(&"Idle")
	
	body.freeze = false
	body.collision_layer = initial_layer
	body.collision_mask = initial_mask
	
### Helpers ###
#func get_aim_context() -> Dictionary:
	#-global_basis.z
