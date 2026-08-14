extends RigidBody3D

# ducky.gd
@onready var trail: MeshInstance3D = $Trail

@export var trail_min_speed: float = 5.0  
@export var trail_max_speed: float = 60.0  

#func _ready() -> void:
	#trail.top_level = true
#
#func _physics_process(_delta: float) -> void:
	##trail.global_position = global_position
#
	#var speed := linear_velocity.length()
	#var factor := clampf(inverse_lerp(trail_min_speed, trail_max_speed, speed), 0.0, 1.0)
	#trail.set_instance_shader_parameter("speed_factor", factor)
