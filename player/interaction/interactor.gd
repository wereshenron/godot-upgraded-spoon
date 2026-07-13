extends Node3D
class_name Interactor

@export_group("Node References")
@export var pickup_raycast: RayCast3D
@export var aim_raycast: RayCast3D
@export var camera: Camera3D

@export_group("Object Handling")
@export var hold_offset: Vector3
@export var throw_offset: Vector3
@export var base_follow_speed: float = 10.0
@export var aim_follow_speed: float = 20.0
## How much influence mass has over following
@export var mass_influence: float = 1.5

@export_group("Timing")
@export var pickup_cooldown: float = 0.25
@export var throw_cooldown: float = 0.33
@export var _min_to_throw_msec: int = 0

@export_group("Throwing")
@export var max_charge_time: float = 1.5

var player_stats: PlayerStats
var _is_aiming: bool = false
var _charge_time: float = 0.0
var _current_target: Grabbable = null
var _object_held: Grabbable = null
var _pickup_locked_until_msec: int = 0

func _ready() -> void:
	pickup_raycast.add_exception(owner)
	

func _physics_process(delta: float) -> void:
	if _object_held:
		if _is_aiming and _can_throw():
			_charge_time = min(_charge_time + delta, max_charge_time)
		_update_held_object(delta)

# Main raycast "seeing the thing initially" logic
func _process(_delta: float) -> void:
	var new_target: Grabbable = null

	if pickup_raycast.is_colliding() and _can_pickup():
		var body = pickup_raycast.get_collider()
		if body.is_in_group("Grabbable"):
			var candidate = body.get_node("Grabbable")
			if candidate != _object_held:
				new_target = candidate
		
	if new_target == _current_target:
		return
	
	if _current_target and _current_target != _object_held:
		_current_target.set_highlighted(false)

	_current_target = new_target

	if _current_target:
		if _current_target != _object_held:
			_current_target.set_highlighted(true)
		SignalBus.interactable_seen.emit(_current_target)
	else: 
		SignalBus.looked_away.emit()

func _unhandled_input(event):
	if event is InputEventKey and Input.is_action_pressed("pick_up"):
		if _object_held:
			_let_go()
		elif _current_target and _can_pickup():
			_pickup(_current_target)

	if _object_held:
		if Input.is_action_just_pressed("throw"):
			_start_throw_cooldown()
			_charge_time = 0.0
			_is_aiming = true
		if Input.is_action_just_released("throw") and _is_aiming and _can_throw():
			_throw()
		if Input.is_action_just_released("throw") and !_can_throw():
			_is_aiming = false
		if Input.is_action_just_pressed("let_go"):
			_let_go()


func _update_held_object(delta: float) -> void:
	var forward = - camera.global_basis.z
	var right = camera.global_basis.x
	var up = camera.global_basis.y

	var offset = hold_offset.lerp(throw_offset, float(_is_aiming and _can_throw()))

	var hold_target: Vector3 = camera.global_position \
		+ forward * offset.z \
		+ right * offset.x \
		+ up * offset.y

	var mass: float = 1.0
	var object_mass = _object_held.body.get("mass")

	if object_mass != null:
		mass = object_mass

	var speed = aim_follow_speed if _is_aiming and _can_throw() else base_follow_speed

	var follow_speed: float = speed / (1.0 + mass * mass_influence)

	_object_held.body.global_position = _object_held.body.global_position.lerp(
		hold_target,
		clampf(follow_speed * delta, 0.0, 1.0)
	)


func _pickup(grabbable: Grabbable) -> void:
	if !grabbable or _object_held:
		return
	
	grabbable.set_highlighted(false)
	grabbable.set_should_hover(true)
	_object_held = grabbable
	_current_target = null
	SignalBus.looked_away.emit()


func _let_go() -> void:
	if !_object_held: return
	if !_object_held.body.has_method("set_freeze_enabled"): return
	
	_object_held.set_should_hover(false)
	_reset_throwing()


func _throw() -> void:
	if !_object_held:
		return
		
	if !_object_held.body.has_method("set_freeze_enabled"):
		_reset_throwing()
		return

	var body : RigidBody3D = _object_held.body

	var charge_ratio = _charge_time / max_charge_time

	var aim_origin = camera.global_position
	var aim_dir = - camera.global_basis.z
	var aim_point = aim_origin + aim_dir * 1000.0

	if aim_raycast.is_colliding() and aim_raycast.get_collider() != _object_held.body:
		aim_point = aim_raycast.get_collision_point()

	var throw_direction = (aim_point - _object_held.body.global_position).normalized()
	
	_object_held.throw(throw_direction, charge_ratio, player_stats.strength_multiplier)
	_reset_throwing()


func _reset_throwing():
	_start_pickup_cooldown()
	_object_held = null
	_current_target = null
	_is_aiming = false
	_charge_time = 0.0

func _can_pickup() -> bool:
	return Time.get_ticks_msec() >= _pickup_locked_until_msec
	
func _can_throw() -> bool:
	return Time.get_ticks_msec() >= _min_to_throw_msec

func _start_pickup_cooldown() -> void:
	_pickup_locked_until_msec = Time.get_ticks_msec() + int(pickup_cooldown * 1000)
	
func _start_throw_cooldown() -> void:
	_min_to_throw_msec = Time.get_ticks_msec() + int(throw_cooldown * 1000)
