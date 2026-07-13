extends Area3D

@export var spawn_point : Marker3D
@export var collider_x : int = 100
@export var collider_z : int = 100

@onready var collider = $CollisionShape3D

#func _ready() -> void:
	#collider.shape.size.x = collider_x
	#collider.shape.size.z = collider_z

func _on_body_entered(body: Node3D) -> void:
	if body is RigidBody3D:
		body.linear_velocity = Vector3.ZERO
		body.angular_velocity = Vector3.ZERO
		
	body.global_transform = spawn_point.global_transform
