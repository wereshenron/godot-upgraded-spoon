extends Node3D

#@onready var terrain_scene = preload("res://terrain/hilly_terrain.tscn").instantiate()

func _on_teleporter_body_entered(body: Node3D) -> void:
	call_deferred("change_scene")
	
func change_scene():
	get_tree().change_scene_to_file("res://terrain/hilly_terrain.tscn")
