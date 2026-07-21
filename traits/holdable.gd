class_name Holdable
extends Node3D

@onready var outline_shader_mat: ShaderMaterial = preload("res://materials/outline.tres")
@onready var body: RigidBody3D = get_parent()

@export var geometry_list: Array[GeometryInstance3D]
## Threshold to decide when to turn off continuous collision detection
@export var _movement_lower_threshold: float = 0.33

var _tween: Tween
var _movement: float
var _has_spiked: bool = false
var _mesh_instances: Array[MeshInstance3D]
var is_aiming = false

signal released

func _ready() -> void:
	var found := find_children("*", "MeshInstance3D", true, false)
	_mesh_instances.assign(found)
	body.add_to_group("Holdable")

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
			_start_tween(blend_val, 0.0, func():
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

func set_should_hover(is_hovering: bool) -> void:
	body.axis_lock_linear_y = is_hovering

#### Holdable contract — override in subclasses ####

## Called every physics frame while held, to compute Interactor's follow offset.
## Default: no offset (subclasses like Throwable override to add aim-shift).
func get_hold_offset() -> Vector3:
	return Vector3.ZERO

func get_follow_speed(base_speed: float) -> float:
	return base_speed

@warning_ignore("standalone_expression")
func primary_pressed(_aim_context: Callable = func(): null) -> void:
	pass

@warning_ignore("standalone_expression")
func primary_held(_delta: float, _aim_context: Callable = func(): null) -> void:
	pass

@warning_ignore("standalone_expression")
func primary_released(_aim_context: Callable = func(): null) -> void:
	pass
	
	@warning_ignore("standalone_expression")
func secondary_pressed(_aim_context: Callable = func(): null) -> void:
	pass

@warning_ignore("standalone_expression")
func secondary_held(_delta: float, _aim_context: Callable = func(): null) -> void:
	pass

@warning_ignore("standalone_expression")
func secondary_released(_aim_context: Callable = func(): null) -> void:
	pass

func can_use() -> bool:
	return true

func update_hold(_hold_pivot: Node3D, _delta: float) -> void:
	pass

#### Helpers ####

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
