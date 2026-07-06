class_name Grabbable
extends Node3D

@onready var outline_shader_mat: ShaderMaterial = preload("res://materials/outline.tres")
@onready var body: RigidBody3D = get_parent()

@export var geometry : GeometryInstance3D
## Threshold to decide when to turn off continuous collision detection
@export var _movement_lower_threshold : float = 0.33

@export_group("Throwing")
@export var min_throw_force : float
@export var max_throw_force : float

var _tween: Tween
var _movement: float
var _has_spiked: bool = false

signal looked_at(target)
signal looked_away

func _ready() -> void:
	looked_at.connect(_look_at)
	looked_away.connect(func(): set_highlighted(false))
	
func _process(_delta: float) -> void:
	if body.continuous_cd == false:
		_has_spiked = false
		return
		
	_movement = clampf(body.linear_velocity.length(), 0.0, 1.0)
	
	if _movement > _movement_lower_threshold:
		_has_spiked = true
	elif _has_spiked:
		print('saving lives')
		body.continuous_cd = false
		_has_spiked = false

func throw(direction: Vector3, charge_ratio: float, strength_mult: float = 1.0) -> void:
	if not body.has_method("set_freeze_enabled"):
		return
	
	var force = lerp(min_throw_force, max_throw_force, charge_ratio) * strength_mult
	body.continuous_cd = true
	body.set_freeze_enabled(false)
	body.apply_central_impulse(force * direction)
	body.angular_velocity = Vector3(randf(), randf(), randf())

func _look_at(target) -> void:
	set_highlighted(true)
	SignalBus.interactable_seen.emit(target)

func set_highlighted(active: bool) -> void:
	if geometry != null:
		if active:
			# print('setting active')
			# Apply overlay first, then tween blend 0 -> 1
			geometry.material_overlay = outline_shader_mat
			_start_tween(0.0, 1.0)
		else: 
			# print('setting inactive')
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
