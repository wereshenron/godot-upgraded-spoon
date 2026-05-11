class_name Grabbable
extends Node3D

@onready var outline_shader_mat: ShaderMaterial = preload("res://materials/outline.tres")

var _tween: Tween

func set_highlighted(active: bool) -> void:
	var meshes = get_parent().find_children("*", "GeometryInstance3D", true, false)

	if active:
		# Apply overlay first, then tween blend 0 -> 1
		for mesh in meshes:
			mesh.material_overlay = outline_shader_mat
		_start_tween(0.0, 1.0)
	else:
		# Tween blend 1 -> 0, then remove overlay once done
		_start_tween(1.0, 0.0, func():
			for mesh in meshes:
				mesh.material_overlay = null
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
