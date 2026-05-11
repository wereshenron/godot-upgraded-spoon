extends RigidBody3D

@export var max_random_ang_velocity: float = 1 

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	angular_velocity = Vector3(randf(), randf(), randf())
