class_name Shootable
extends Holdable

@export_category("Node References")
@export var state_machine: StateMachine

@export_category("Aiming")
@export var aim_offset: Vector3
@export var aim_transition_time: float = 0.15  # seconds to fully reach aim_offset

@export_category("Firing")
@export var recoil_amount: Vector3
@export var snap: float
@export var recoil_speed: float
@export var fire_rate: float
@export var fire_velocity: float = 200.0

@onready var aim_raycast: RayCast3D = get_viewport().get_camera_3d().get_node("AimRaycast")
@onready var bullet: PackedScene = preload("res://playground/ball.tscn")
@onready var bullet_pivot: Node3D = body.get_node('BulletPivot')

var aim_blend: float = 0.0  # 0 = hold_offset, 1 = aim_offset
var initial_layer
var initial_mask
var current_recoil_offset: Vector3 = Vector3.ZERO
var target_recoil_offset: Vector3 = Vector3.ZERO
var direction: Vector3 = Vector3.ZERO

func _ready() -> void:
	super._ready()
	
	# Initialize State
	state_machine.add_state(&"Idle", ShootableIdle.new())
	state_machine.add_state(&"Firing", ShootableFiring.new())
	state_machine.start(&"Idle")
	initial_layer = body.collision_layer
	initial_mask = body.collision_mask
	
	# Set up signal handlers
	grabbed.connect(_on_grabbed)
	released.connect(_on_released)
	
@warning_ignore("standalone_expression")
func primary_pressed(_aim_context: Callable = func(): null) -> void:
	state_machine.current_state.primary_pressed(_aim_context)

@warning_ignore("standalone_expression")
func secondary_pressed(_aim_context: Callable = func(): null) -> void:
	is_aiming = true
	SignalBus.show_reticle.emit(false)

@warning_ignore("standalone_expression")
func secondary_released(_aim_context: Callable = func(): null) -> void:
	is_aiming = false
	SignalBus.show_reticle.emit(true)

func update_hold(hold_pivot: Node3D, delta: float) -> void:
	if !hold_pivot: return

	# advance the aim transition
	aim_blend = move_toward(aim_blend, is_aiming, delta / aim_transition_time)
	
	# advance the recoil transition
	target_recoil_offset = target_recoil_offset.move_toward(Vector3.ZERO, recoil_speed * delta)
	current_recoil_offset = current_recoil_offset.lerp(target_recoil_offset, snap * delta)

	var forward = -hold_pivot.global_basis.z
	var right = hold_pivot.global_basis.x
	var up = hold_pivot.global_basis.y
	var offset = get_hold_offset()  # now uses aim_blend, not a hard 0/1

	var hold_target: Vector3 = hold_pivot.global_position \
		+ forward * (offset.z + current_recoil_offset.z) \
		+ right * (offset.x + current_recoil_offset.x) \
		+ up * (offset.y + current_recoil_offset.y)
	
	# Aiming
	var aim_origin = aim_raycast.global_position
	var aim_dir = -aim_raycast.global_basis.z
	var aim_point = aim_origin + aim_dir * 1000
	direction = (aim_point - body.global_position).normalized()
	var target_basis = Basis.looking_at(direction, hold_pivot.global_basis.y)
	var yaw_basis := Basis(Vector3.UP, current_recoil_offset.y)
	var aim_with_yaw: Basis = yaw_basis * target_basis

	# pitch kick: local rotation around the (now-yawed) right axis
	var pitch_basis := Basis(aim_with_yaw.x, current_recoil_offset.x)
	var target_basis_final := pitch_basis * aim_with_yaw
	
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
		target_basis_final,
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
func get_hold_offset() -> Vector3:
	return hold_offset.lerp(aim_offset, aim_blend)

func get_follow_speed(base_speed: float) -> float:
	if is_aiming:
		return aim_follow_speed
	
	return base_speed
	
func shoot() -> void:
	apply_recoil_kick()
	var new_bullet: RigidBody3D = bullet.instantiate()
	get_tree().root.add_child(new_bullet)
	
	new_bullet.continuous_cd = true
	new_bullet.global_position = bullet_pivot.global_position
	new_bullet.apply_central_impulse(direction * fire_velocity)
	
func apply_recoil_kick() -> void:
	var recoil_applied = Vector3(
		randf_range(recoil_amount.x / 2, recoil_amount.x),
		randf_range(recoil_amount.y / 2, recoil_amount.y),
		randf_range(recoil_amount.z / 2, recoil_amount.z),
	)

	SignalBus.recoil_kicked.emit(recoil_applied)
	target_recoil_offset += recoil_applied
