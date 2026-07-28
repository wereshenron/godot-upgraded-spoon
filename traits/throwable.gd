class_name Throwable
extends Holdable

@export var min_throw_force: float
@export var max_throw_force: float
@export var angular_velocity_scale: float
@export_range(-180.0, 180.0, 1.0, "degrees") var spin_twist_degrees: float = 0.0
@export var throw_offset: Vector3
@export var max_charge_time: float = 1.5

var _charge_time: float = 0.0
var strength_mult: float = 1.0 # set externally, e.g. from player_stats

func _ready() -> void:
	# Signal Connections
	grabbed.connect(on_grabbed)


#### Signal Handlers ####
func on_grabbed() -> void:
	set_should_hover(true)
	set_highlighted(false)
	SignalBus.looked_away.emit()

## Holdable overrides

func get_hold_offset() -> Vector3:
	return hold_offset.lerp(throw_offset, float(is_aiming))

func get_follow_speed(base_speed: float) -> float:
	return aim_follow_speed if is_aiming else base_speed

@warning_ignore("standalone_expression")
func primary_pressed(_aim_context: Callable = func(): null) -> void:
	if is_aiming:
		var context = _aim_context.call()
		_do_throw(context.direction, context.strength_mult)
	is_aiming = false
	_charge_time = 0.0
	
@warning_ignore("standalone_expression")
func secondary_pressed(_aim_context: Callable = func(): null) -> void:
	is_aiming = true
	_charge_time = 0.0
	
@warning_ignore("standalone_expression")
func secondary_held(delta: float, _aim_context: Callable = func(): null) -> void:
	if is_aiming:
		_charge_time = min(_charge_time + delta, max_charge_time)
	
@warning_ignore("standalone_expression")
func secondary_released(_aim_context: Callable = func(): null) -> void:
	is_aiming = false
	_charge_time = 0.0

func update_hold(hold_pivot: Node3D, delta: float) -> void:
	if !hold_pivot: return
	
	var forward = - hold_pivot.global_basis.z
	var right = hold_pivot.global_basis.x
	var up = hold_pivot.global_basis.y
	var offset = get_hold_offset()

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

## Throwable's own physics action on the body.
func _do_throw(direction: Vector3, strength: float = 1.0) -> void:
	var charge_ratio = _charge_time / max_charge_time
	var force = lerp(min_throw_force, max_throw_force, charge_ratio) * strength
	var relative_angular_velocity = _get_relative_angular_velocity(direction)
	var charged_angular_velocity = Vector3.ZERO.lerp(relative_angular_velocity, charge_ratio)

	released.emit()
	body.continuous_cd = true
	body.axis_lock_linear_y = false
	body.apply_central_impulse(force * direction)
	body.angular_velocity = charged_angular_velocity

func _get_relative_angular_velocity(direction: Vector3) -> Vector3:
	var dir_norm = direction.normalized()
	var base_axis = Vector3.UP.cross(dir_norm).normalized()
	var spin_axis = base_axis.rotated(dir_norm, deg_to_rad(spin_twist_degrees))
	return spin_axis * angular_velocity_scale
