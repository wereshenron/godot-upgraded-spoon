extends Node3D

@onready var bonesimulator = $Armature/Skeleton3D/PhysicalBoneSimulator3D

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	bonesimulator.physical_bones_start_simulation()
