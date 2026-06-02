extends Node3D

@export var raycast : RayCast3D
@export var camera : Camera3D
@export var hold_distance : float = 2.0       # How far in front of camera to hold object
@export var base_follow_speed : float = 10.0  # Higher = snappier, lower = laggier
@export var mass_influence : float = 1.5      # How much mass slows the follow speed

var _current_target: Node3D = null
var _object_held: Node3D = null

func _ready() -> void:
	raycast.add_exception(owner)
	

func _physics_process(delta: float) -> void:
	if _object_held:
		_update_held_object(delta)


func _process(_delta: float) -> void:
	if raycast and raycast.is_colliding():
		var target: Node3D = raycast.get_collider()
		if target.is_in_group("Grabbable") and target != _current_target:
			_clear_highlight()
			_set_highlight(target, true)
			_current_target = target
			return
	else:
		_clear_highlight()
		_current_target = null


func _unhandled_input(event):
	if event is InputEventKey and Input.is_action_pressed("pick_up"):
		if _object_held:
			_let_go()
		elif _current_target:
			_pickup(_current_target)


func _update_held_object(delta: float) -> void:
	# Target position: a point directly in front of the camera
	var hold_target : Vector3 = camera.global_position + (-camera.global_basis.z * hold_distance)

	# Read the object's mass to scale lag — falls back to 1.0 if no mass property
	var mass : float = 1.0
	if _object_held.get("mass") != null:
		mass = _object_held.get("mass")

	# Heavier objects get a lower follow speed, making them feel sluggish
	var follow_speed : float = base_follow_speed / (1.0 + mass * mass_influence)

	# Smoothly interpolate the object toward the hold point
	_object_held.global_position = _object_held.global_position.lerp(
		hold_target,
		clamp(follow_speed * delta, 0.0, 1.0)
	)


func _clear_highlight() -> void:
	if _current_target:
		var grabbable = _current_target.get_node_or_null("Grabbable")
		if grabbable and grabbable.has_method("set_highlighted"):
			grabbable.set_highlighted(false)


func _set_highlight(target: Node3D, active: bool) -> void:
	var grabbable = target.get_node_or_null("Grabbable")
	if grabbable and grabbable.has_method("set_highlighted"):
		grabbable.set_highlighted(active)


func _pickup(target: Node3D) -> void:
	if !target or _object_held:
		return
	if target.has_method("set_freeze_enabled"):
		target.set_freeze_enabled(true)
	else:
		return
	_object_held = target
	# No reparenting — we move it manually in _update_held_object


func _let_go() -> void:
	if !_object_held:
		return
	if _object_held.has_method("set_freeze_enabled"):
		_object_held.set_freeze_enabled(false)
	else:
		return
	_object_held = null
