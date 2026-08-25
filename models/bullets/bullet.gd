class_name Bullet
extends Node3D


@onready var tracer_material: Material = preload("res://textures/materials/trail.tres")

var velocity: Vector3
var gravity_scale: float = 0.0  # set >0 if you want drop, 0 for flat trajectory
var damage: float = 10.0
var lifetime: float = 3.0
var _age: float = 0.0

signal on_hit(result)

func _physics_process(delta: float) -> void:
	_age += delta
	if _age > lifetime:
		queue_free()
		return

	var start := global_position
	velocity += Vector3.DOWN * gravity_scale * delta
	var end := start + velocity * delta
	
	draw_tracer(start, end)

	var space_state := get_world_3d().direct_space_state
	var query := PhysicsRayQueryParameters3D.create(start, end)
	query.exclude = [self]  # add shooter here too if needed
	var result := space_state.intersect_ray(query)

	if result:
		global_position = result.position
		_on_hit(result)
		queue_free()
	else:
		global_position = end
		
func _on_hit(result: Dictionary):
	#print(result)
	var collider = result.get("collider")
	var hit_position = result.get("position")
	var impulse: Vector3 = velocity.normalized() * damage
	var offset: Vector3 =  hit_position - collider.global_position
	if collider is RigidBody3D:
		collider.apply_impulse(impulse, offset)
		
	elif collider is PhysicalBone3D:
		var bone: PhysicalBone3D = collider
		var skeleton: PhysicalBoneSimulator3D = bone.get_parent() as PhysicalBoneSimulator3D
		if skeleton:
			# make sure this bone (and ideally its neighbors) are actually
			# being simulated before an impulse can do anything
			skeleton.physical_bones_start_simulation([bone.get('bone_name')])
			
		#var offset: Vector3 = hit_position - bone.global_position
		PhysicsServer3D.body_apply_impulse(bone.get_rid(), impulse, offset)
		skeleton.physical_bones_start_simulation()
		
func draw_tracer(start: Vector3, end: Vector3) -> void:
	var tracer_mesh := ImmediateMesh.new()
	tracer_mesh.surface_begin(Mesh.PRIMITIVE_LINES, tracer_material)
	tracer_mesh.surface_set_color(Color(1,1,1, 1))
	tracer_mesh.surface_set_uv(Vector2(0,0))
	tracer_mesh.surface_add_vertex(start)
	tracer_mesh.surface_set_color(Color(1,1,1, 0))
	tracer_mesh.surface_set_uv(Vector2(1,0))
	tracer_mesh.surface_add_vertex(end)
	tracer_mesh.surface_end()

	var tracer_instance := MeshInstance3D.new()
	tracer_instance.mesh = tracer_mesh
	tracer_instance.material_override = tracer_material.duplicate()
	get_tree().root.add_child(tracer_instance)

	var tween := tracer_instance.create_tween()
	tween.tween_method(
		func(p): tracer_instance.material_override.set_shader_parameter("progress", p),
		0.5, 1.0, .22
	)
	tween.tween_callback(tracer_instance.queue_free)
