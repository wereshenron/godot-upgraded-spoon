extends Node3D

@onready var skeleton = $ShrekWazowski/Skeleton3D

@export var idle_speed: float = 1.5
@export var idle_amount: float = 15.0  # degrees
@export var bone_name: String = "mixamorig_Head"

func _process(delta: float) -> void:
	var bone_idx = skeleton.find_bone(bone_name)
	if bone_idx == -1:
		return

	var rest_pose: Transform3D = skeleton.get_bone_rest(bone_idx)

	# sin oscillates -1 to 1, scaled to your desired degree range
	var angle := sin(Time.get_ticks_msec() * 0.001 * idle_speed) * deg_to_rad(idle_amount)

	var rotation := Basis(Vector3.RIGHT, angle)
	var blended := rest_pose.basis * rotation

	skeleton.set_bone_pose_rotation(bone_idx, blended.get_rotation_quaternion())
