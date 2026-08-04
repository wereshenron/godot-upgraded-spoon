extends Node3D
class_name Interactable

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