extends Node3D
class_name Interactor

@export_group("Node References")
@export var pickup_raycast: RayCast3D
@export var aim_raycast: RayCast3D
@export var camera: Camera3D

@export_group("Object Handling")
@export var hold_offset: Vector3
@export var hold_pivot: Node3D
@export var throw_offset: Vector3
@export var base_follow_speed: float = 10.0
@export var aim_follow_speed: float = 20.0
## How much influence mass has over following
@export var mass_influence: float = 1.5

@export_group("Timing")
@export var pickup_cooldown: float = 0.25
@export var throw_cooldown: float = 0.33
@export var _min_to_throw_msec: int = 0

var player_stats: PlayerStats
var _is_aiming: bool = false
var _current_target: Holdable = null
var _object_held: Holdable = null
var _pickup_locked_until_msec: int = 0

func _ready() -> void:
	pickup_raycast.add_exception(owner)
	

func _physics_process(delta: float) -> void:
	if _object_held:
		_object_held.on_use_held(delta, _get_aim_context)
		_object_held.update_hold(hold_pivot, delta, _is_aiming)

# Main raycast "seeing the thing initially" logic
func _process(_delta: float) -> void:
	var new_target: Holdable = null

	if pickup_raycast.is_colliding() and _can_pickup():
		var body = pickup_raycast.get_collider()
		if body.is_in_group("Holdable"):
			var candidate = _get_holdable(body)
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
		if Input.is_action_just_pressed("use_primary"):
			_is_aiming = true
			_object_held.on_use_pressed(_get_aim_context)
		if Input.is_action_just_released("use_primary"):
			_object_held.on_use_released(_get_aim_context)
			_is_aiming = false
		if Input.is_action_just_pressed("let_go"):
			_let_go()


func _pickup(holdable: Holdable) -> void:
	if !holdable or _object_held:
		return
	
	holdable.set_highlighted(false)
	holdable.set_should_hover(true)
	_object_held = holdable
	_current_target = null
	SignalBus.looked_away.emit()
	_object_held.released.connect(_on_held_object_released)


func _let_go() -> void:
	if !_object_held: return
	if !_object_held.body is RigidBody3D: return
	_object_held.set_should_hover(false)
	_object_held.released.emit()
	_reset_throwing()

func _get_aim_context() -> Dictionary:
	var aim_origin = camera.global_position
	var aim_dir = - camera.global_basis.z
	var aim_point = aim_origin + aim_dir * 1000.0

	if aim_raycast.is_colliding() and aim_raycast.get_collider() != _object_held.body:
		aim_point = aim_raycast.get_collision_point()

	var direction = (aim_point - _object_held.body.global_position).normalized()
	return {
		"direction" : direction,
		"strength_mult" : player_stats.strength_multiplier,
		"is_aiming": _is_aiming
	}


func _reset_throwing():
	_start_pickup_cooldown()
	_object_held = null
	_current_target = null
	_is_aiming = false

func _on_held_object_released() -> void:
	if !_object_held: return
	_object_held.released.disconnect(_on_held_object_released)
	_reset_throwing()


func _can_pickup() -> bool:
	return Time.get_ticks_msec() >= _pickup_locked_until_msec
	
func _can_throw() -> bool:
	return Time.get_ticks_msec() >= _min_to_throw_msec

func _start_pickup_cooldown() -> void:
	_pickup_locked_until_msec = Time.get_ticks_msec() + int(pickup_cooldown * 1000)
	
func _start_throw_cooldown() -> void:
	_min_to_throw_msec = Time.get_ticks_msec() + int(throw_cooldown * 1000)

func _get_holdable(body: RigidBody3D) -> Holdable:
	for child in body.get_children():
		if child is Holdable:
			return child
	return null
