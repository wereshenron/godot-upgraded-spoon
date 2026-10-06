extends Marker3D
class_name SpawnPoint

@export var spawn_id : String = "default"

func _ready() -> void:
	GameManager.Instance.
