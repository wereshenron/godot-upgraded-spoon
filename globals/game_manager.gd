extends Node

@onready var _global_scenes : Array[PackedScene] = [
	preload("res://hud/hud.tscn")
]

@onready var _player_scene : PackedScene = preload("res://player/Player.tscn")

var _player : Node3D = null

func _ready() -> void:
	_add_global_instances()
	_spawn_player("default")
	
func _add_global_instances() -> void:
	for scene in _global_scenes:
		var instance = scene.instantiate()
		add_child(instance)
		
func _spawn_player(spawn_id: String) -> void:
	if _player == null:
		_player = _player_scene.instantiate()
		add_child(_player)
		
	var target = _find_spawn_point(spawn_id)
	if target:
		_player.global_position = target.global_position
		_player.rotation = target.rotation

func _find_spawn_point(spawn_id: String) -> Node3D:
	for spawn_point in get_tree().get_nodes_in_group("SpawnPoint"):
		if spawn_point.spawn_id == spawn_id:
			return spawn_point
	var fallback := get_tree().get_nodes_in_group("SpawnPoint")
	if not fallback: 
		return null
	return fallback[0]
