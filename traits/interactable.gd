extends Node3D
class_name Interactable

@export var geometry_list: Array[GeometryInstance3D]
@onready var outline_shader_mat: ShaderMaterial = preload("res://textures/materials/outline.tres").duplicate()

var _tween: Tween

func _ready() -> void:
	get_parent().add_to_group("Interactable")
	SignalBus.looked_away.connect(func(): set_highlighted(false))

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

func can_interact(_interactor: Interactor) -> bool:
	return true

func interact(_interactor: Interactor, _message: Dictionary = {}) -> void:
	pass
