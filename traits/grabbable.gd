class_name Grabbable
extends Node3D

@onready var outline_shader_mat: ShaderMaterial = preload("res://materials/outline.tres")
@onready var body: RigidBody3D = get_parent()

@export var geometry_list : Array[GeometryInstance3D]
## Threshold to decide when to turn off continuous collision detection
@export var _movement_lower_threshold : float = 0.33

@export_group("Throwing")
@export var min_throw_force : float
@export var max_throw_force : float
@export var angular_velocity_scale : float
@export_range(-180.0, 180.0, 1.0, "degrees") var spin_twist_degrees : float = 0.0

var _tween: Tween
var _movement: float
var _has_spiked: bool = false
var _mesh_instances: Array[MeshInstance3D]

func _ready() -> void:
	var found := find_children("*", "MeshInstance3D", true, false)
	_mesh_instances.assign(found)
	
func _physics_process(_delta: float) -> void:
	_handle_ccd()

#### UI Interactions ####

func _look_at(target) -> void:
	set_highlighted(true)
	SignalBus.interactable_seen.emit(target)

func set_highlighted(active: bool) -> void:
	for geometry in geometry_list:
		if geometry == null:
			return
		if active:
			geometry.material_overlay = outline_shader_mat
			_start_tween(0.0, 1.0)
		else: 
			var blend_val = outline_shader_mat.get_shader_parameter("blend")
			_start_tween(blend_val, 0.0, func ():
				geometry.material_overlay = null
			)

func _start_tween(from: float, to: float, on_complete: Callable = Callable()) -> void:
	if _tween:
		_tween.kill()
	_tween = create_tween()
	_tween.tween_method(_set_blend, from, to, 0.3)
	if on_complete.is_valid():
		_tween.tween_callback(on_complete)

func _set_blend(value: float) -> void:
	outline_shader_mat.set_shader_parameter("blend", value)

#### Physics Interactions ####

## Grabbable's throw() handles the actual physics actions on the body
func throw(direction: Vector3, charge_ratio: float, strength_mult: float = 1.0) -> void:
	var force = lerp(min_throw_force, max_throw_force, charge_ratio) * strength_mult
	var relative_angular_velocity = _get_relative_angular_velocity(direction)
	#print(relative_angular_velocity)
	var charged_angular_velocity = Vector3.ZERO.lerp(relative_angular_velocity, charge_ratio)
	
	body.continuous_cd = true
	body.axis_lock_linear_y = false
	body.apply_central_impulse(force * direction)
	body.angular_velocity = charged_angular_velocity

func set_should_hover(is_hovering: bool) -> void:
	body.axis_lock_linear_y = is_hovering
	
	
#### Helpers #### 

## Thrown objects "forward-down" angular velocity rotation is relative to 
## the thrown direction - it is scaled directly related to whichever axis 
## is closest to 0.
func _get_relative_angular_velocity(direction: Vector3) -> Vector3:
	var dir_norm = direction.normalized()
	var base_axis = Vector3.UP.cross(dir_norm).normalized()
	var spin_axis = base_axis.rotated(dir_norm, deg_to_rad(spin_twist_degrees))
	return spin_axis * angular_velocity_scale
	
func _handle_ccd() -> void:
	if body.continuous_cd == false:
		_has_spiked = false
		return
		
	_movement = clampf(body.linear_velocity.length(), 0.0, 1.0)
	
	if _movement > _movement_lower_threshold:
		_has_spiked = true
	elif _has_spiked:
		body.continuous_cd = false
		_has_spiked = false
