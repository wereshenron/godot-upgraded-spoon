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
var current_target: Interactable = null
var new_target: Interactable = null
var object_held: Holdable = null

func _ready() -> void:
	pickup_raycast.add_exception(owner)
	
	state_machine.add_state(&"Idle", InteractorIdleState.new())
	state_machine.add_state(&"Holding", InteractorHoldingState.new())
	state_machine.start(&"Idle")

func _physics_process(_delta: float) -> void:
	object_held = state_machine.current_state.get("_object_held")
	new_target = null

	if pickup_raycast.is_colliding():
		var body = pickup_raycast.get_collider()
		if body.is_in_group("Interactable"):
			new_target = get_interactable(body)

	# Return early if the "new target" and current target are not null, and either
	#	a. new target IS the current target
	#	b. new target is currently held
	if (new_target && current_target) && \
		(new_target == current_target or new_target == object_held):
		return

	#if current_target:
		#current_target.set_highlighted(false)
		#SignalBus.looked_away.emit()

	current_target = new_target

	if current_target:
		if object_held and current_target.body.is_in_group("Holdable"):
			return
		
		current_target.set_highlighted(true)
		SignalBus.interactable_seen.emit(current_target)
	else:
		SignalBus.looked_away.emit()

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("interact") and current_target:
		current_target.interact(self, {"target": current_target})

func get_interactable(body: RigidBody3D) -> Interactable:
	for child in body.get_children():
		if child is Interactable:
			return child
	return null
