class_name Interactor
extends Node3D

@export_group("Node References")
@export var pickup_raycast: RayCast3D
@export var aim_raycast: RayCast3D
@export var camera: Camera3D
@export var hold_pivot: Node3D
@export var state_machine: StateMachine

@export_group("Timing")
@export var pickup_cooldown: float = 0.25

var player_stats: PlayerStats

func _ready() -> void:
	pickup_raycast.add_exception(owner)
	
	state_machine.add_state(&"Idle", InteractorIdleState.new())
	state_machine.add_state(&"Holding", InteractorHoldingState.new())
	state_machine.start(&"Idle")

func get_holdable(body: RigidBody3D) -> Holdable:
	for child in body.get_children():
		if child is Holdable:
			return child
	return null
