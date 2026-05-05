extends CharacterBody3D

@onready var _camera := get_node("CameraPivot/Camera3D") as Camera3D
@onready var _camera_pivot := get_node("CameraPivot") as Node3D

@export_range(0.0, 1.0) var mouse_sensitivity = 0.01
@export_range(0.0, 50.0) var jump_force = 25.0;
@export_range(1.0, 2.0) var sprint_multiplier = 1.6

@export var tilt_limit = deg_to_rad(75)
@export var turn_speed = 0.5 as float
@export var speed = 14
@export var fall_acceleration = 75

var target_velocity = Vector3.ZERO

func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		_camera_pivot.rotation.x -= event.relative.y * mouse_sensitivity
		# Prevent the camera from rotating too far up or down.
		_camera_pivot.rotation.x = clampf(_camera_pivot.rotation.x, -tilt_limit, tilt_limit)
		_camera_pivot.rotation.y += -event.relative.x * mouse_sensitivity

func _physics_process(delta):
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

	if direction != Vector3.ZERO:
		var camera_basis = _camera.global_transform.basis
		camera_basis.y = Vector3.UP  # force y-axis to be up
		camera_basis = Basis(Vector3.UP, _camera.global_transform.basis.get_euler().y)
		
		var forward = -camera_basis.z.normalized()
		var right = camera_basis.x.normalized()
		direction = (right * direction.x + forward * direction.z).normalized()
		
		var look_direction = Vector3(direction.x, 0, direction.z)
		var current = $Pivot.basis
		var target = Basis.looking_at(look_direction)
		$Pivot.basis = current.slerp(target, delta * turn_speed)

	# Ground Velocity
	target_velocity.x = direction.x * running_speed
	target_velocity.z = direction.z * running_speed
	
	if Input.is_action_just_pressed("jump") and is_on_floor():
		target_velocity.y = jump_force

	# Vertical Velocity
	if not is_on_floor():
		target_velocity.y = target_velocity.y - (fall_acceleration * delta)
		
	# Moving the Character
	velocity = target_velocity
	move_and_slide()

			
