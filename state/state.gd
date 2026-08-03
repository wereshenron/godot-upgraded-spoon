extends RefCounted
class_name State
## Base class for a single state in a StateMachine. States are plain objects,
## not scene tree nodes - they're registered explicitly via StateMachine.add_state().

signal transitioned(state: State, new_state_id: StringName, msg: Dictionary)

## Set by StateMachine.add_state() - the object this state is driving.
var actor: Node3D

func enter(_msg: Dictionary = {}) -> void:
	pass

func exit() -> void:
	pass

func update(_delta: float) -> void:
	pass

func physics_update(_delta: float) -> void:
	pass

func handle_input(_event: InputEvent) -> void:
	pass
