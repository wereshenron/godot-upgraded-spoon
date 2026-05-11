class_name Grabbable
extends Node3D

@onready var outline_shader_mat: ShaderMaterial = preload("res://materials/outline.tres")

func set_highlighted(active: bool) -> void:
	print('calling from grabbable')
	for mesh in get_parent().find_children("*", "GeometryInstance3D", true, false):
		mesh.material_overlay = outline_shader_mat if active else null
	
