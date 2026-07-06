class_name Grabbable
extends Node3D

@onready var outline_shader_mat: ShaderMaterial = preload("res://materials/outline.tres")
@onready var body: RigidBody3D = get_parent()

@export var geometry : GeometryInstance3D

var _tween: Tween

signal looked_at(target)
signal looked_away

func _ready() -> void:
	looked_at.connect(_look_at)
	looked_away.connect(func(): set_highlighted(false))

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
