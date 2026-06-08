extends Node3D

@export_group("Node References")
@export var pickup_raycast : RayCast3D
@export var aim_raycast : RayCast3D
@export var camera : Camera3D

@export_group("Object Handling")
@export var hold_offset : Vector3
@export var throw_offset: Vector3
@export var base_follow_speed : float = 10.0 
@export var aim_follow_speed : float = 20.0
@export var mass_influence : float = 1.5

@export_group("Throwing")
@export var min_throw_force : float = 5.0
@export var max_throw_force : float = 20.0
@export var max_charge_time : float = 1.5

var _is_aiming: bool = false
var _charge_time: float = 0.0
var _current_target: Node3D = null
var _object_held: Node3D = null

func _ready() -> void:
	pickup_raycast.add_exception(owner)
	

func _physics_process(delta: float) -> void:
	if _object_held:
		if _is_aiming:
			_charge_time = min(_charge_time + delta, max_charge_time)
		_update_held_object(delta)



func _process(_delta: float) -> void:
	if pickup_raycast.is_colliding():
		var target = pickup_raycast.get_collider()
		if target.is_in_group("Grabbable") and target != _current_target:
			if _current_target:
				_current_target.get_node("Grabbable").looked_away.emit()
				print('got here')
			_current_target = target
			_current_target.get_node("Grabbable").looked_at.emit()
			return
		elif !target.is_in_group("Grabbable"): _clear_highlight()
	else: _clear_highlight()


func _unhandled_input(event):
	if event is InputEventKey and Input.is_action_pressed("pick_up"):
		if _object_held:
			_let_go()
		elif _current_target:
			_pickup(_current_target)

	if _object_held:
		if Input.is_action_just_pressed("throw"):
			_is_aiming = true
			_charge_time = 0.0
		if Input.is_action_just_released("throw") and _is_aiming:
			_throw()
		if Input.is_action_just_pressed("let_go"):
			_let_go()


 
func _update_held_object(delta: float) -> void:
	var forward = -camera.global_basis.z
	var right = camera.global_basis.x
	var up = camera.global_basis.y

	var offset = hold_offset.lerp(throw_offset, float(_is_aiming))

	var hold_target : Vector3 = camera.global_position \
		+ forward * offset.z \
		+ right * offset.x \
		+ up * offset.y

	var mass : float = 1.0
	var object_mass = _object_held.get("mass")

	if object_mass != null:
		mass = object_mass

	var speed = aim_follow_speed if _is_aiming else base_follow_speed

	var follow_speed : float = speed / (1.0 + mass * mass_influence)

	_object_held.global_position = _object_held.global_position.lerp(
		hold_target,
		clamp(follow_speed * delta, 0.0, 1.0)
	)


func _clear_highlight() -> void:
	if _current_target:
		var grabbable : Grabbable = _current_target.get_node("Grabbable")
		if grabbable:
			grabbable.looked_away.emit()
		_current_target = null
	


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


func _let_go() -> void:
	if !_object_held:
		return
	if _object_held.has_method("set_freeze_enabled"):
		_object_held.set_freeze_enabled(false)
	else:
		return
	_object_held = null

	_reset_throwing()


func _throw() -> void:
	if !_object_held:
		return

	var charge_ratio = _charge_time / max_charge_time
	var force = lerp(min_throw_force, max_throw_force, charge_ratio)

	var aim_origin = camera.global_position
	var aim_dir = -camera.global_basis.z
	var aim_point = aim_origin + aim_dir * 1000.0

	if aim_raycast.is_colliding() and aim_raycast.get_collider() != _object_held:
		aim_point = aim_raycast.get_collision_point()
		print('set in statement')

	var throw_direciton = (aim_point - _object_held.global_position).normalized()	

	_object_held.set_freeze_enabled(false)
	_object_held.apply_central_force(force * throw_direciton)

	_reset_throwing()


func _reset_throwing():
	_object_held = null
	_is_aiming = false
	_charge_time = 0.0
