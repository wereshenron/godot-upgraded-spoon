extends Area3D

@export var location : Vector3
@export var collider: CollisionShape3D
@export var collider_x : int = 100
@export var collider_z : int = 100

func _ready() -> void:
	collider.scale.x = collider_x
	collider.scale.z = collider_z

func _on_body_entered(body: Node3D) -> void:
	if body.has_method("set_linear_velocity"):
		body.linear_velocity = Vector3.ZERO
		body.angular_velocity = Vector3.ZERO
		
	body.global_position = location
