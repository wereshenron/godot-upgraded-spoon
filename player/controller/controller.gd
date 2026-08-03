extends CharacterBody3D

@onready var _camera : Camera3D = get_node("CameraPivot/Camera3D")
@onready var _camera_pivot : Node3D = get_node("CameraPivot")

@export_range(0.001, 0.01) var mouse_sensitivity : float = 0.003
@export_range(0.0, 50.0) var jump_force : float = 8.0;
@export_range(1.0, 2.0) var sprint_multiplier : float = 1.6
@export_range(0.01, 1.0) var camera_smoothing : float = 0.12

@export var player_stats : PlayerStats = preload("res://player/stats/player_stats.tres")
@export var tilt_limit : float = deg_to_rad(75)
@export var turn_speed : float = 4.8

@export var fall_acceleration : float = 75.0
@export var push_force : float = 5.0

@export var recoil_recover_speed: float = 4.0
@export var recoil_snap: float = 12.0

@onready var speed : float = player_stats.speed
@onready var interactor: Interactor = $Interactor

var target_velocity = Vector3.ZERO
var _target_cam_x := 0.0
var _target_cam_y := 0.0
var _is_sprinting := false
var camera_recoil_offset: Vector3 = Vector3.ZERO
var camera_recoil_target: Vector3 = Vector3.ZERO

func _ready() -> void:
	interactor.player_stats = player_stats
	SignalBus.recoil_kicked.connect(_on_recoil_kicked)

func _on_recoil_kicked(amount: Vector3) -> void:
	camera_recoil_target += amount

# Commented out because this is how you can "run through" objects - keeping for testing 
# func _process(_delta: float) -> void:
# 	set_collision_mask_value(16, !_is_sprinting)

func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		_target_cam_x = clampf(
			_target_cam_x - event.relative.y * mouse_sensitivity, 
			-tilt_limit, 
			tilt_limit
		)
		_target_cam_y += -event.relative.x * mouse_sensitivity

func _physics_process(delta):
	# Aim camera by lerp
	var t := 1.0 - pow(1.0 - camera_smoothing, delta * 60.0)
	_camera_pivot.rotation.x = lerp(_camera_pivot.rotation.x, _target_cam_x, t)
	_camera_pivot.rotation.y = lerp(_camera_pivot.rotation.y, _target_cam_y, t)

	camera_recoil_target = camera_recoil_target.move_toward(Vector3.ZERO, recoil_recover_speed * delta)
	camera_recoil_offset = camera_recoil_offset.lerp(camera_recoil_target, recoil_snap * delta)

	_camera_pivot.rotation.x += camera_recoil_offset.x
	_camera_pivot.rotation.y -= camera_recoil_offset.y
	
	var direction = Vector3.ZERO
	var running_speed = speed

	if Input.is_action_pressed("move_right"):
		direction.x += 1
	if Input.is_action_pressed("move_left"):
		direction.x -= 1
	if Input.is_action_pressed("move_back"):
		direction.z -= 1
	if Input.is_action_pressed("move_forward"):
		direction.z += 1
	if Input.is_action_pressed("sprint"):
		running_speed *= sprint_multiplier
		_is_sprinting = true
	else: _is_sprinting = false

	if direction != Vector3.ZERO:
		var camera_basis = _camera.global_transform.basis
		camera_basis = Basis(Vector3.UP, _camera.global_transform.basis.get_euler().y)
		
		var forward = -camera_basis.z.normalized()
		var right = camera_basis.x.normalized()
		direction = (right * direction.x + forward * direction.z).normalized()
		
		var look_direction = Vector3(direction.x, 0, direction.z)
		var current: Basis = $Pivot.basis.orthonormalized()
		var target: Basis = Basis.looking_at(look_direction).orthonormalized()
		target = Basis(target.x.normalized(), Vector3.UP, target.z.normalized())
		$Pivot.basis = current.slerp(target, delta * turn_speed)

	# Ground Velocity
	target_velocity.x = lerp(target_velocity.x, direction.x * running_speed, t)
	target_velocity.z = lerp(target_velocity.z, direction.z * running_speed, t)
	
	if Input.is_action_just_pressed("jump") and is_on_floor():
		target_velocity.y = jump_force

	# Vertical Velocity
	if not is_on_floor():
		target_velocity.y = lerp(target_velocity.y, target_velocity.y - (fall_acceleration * delta), t)
		
	velocity = target_velocity
	move_and_slide()
