extends Node
class_name StateMachine
## Add this as a child node and drive it entirely from code: register each
## state with an explicit id via add_state(), then call start() with the id
## you want as the default. Nothing about behavior is inferred from the
## scene tree - the full set of valid states and the default are both
## declared in one place, wherever you call add_state()/start().

@export var actor: Node

var current_state: State
var current_state_id: StringName
var _states: Dictionary = {}

func add_state(id: StringName, state: State) -> void:
	state.actor = actor
	state.transitioned.connect(_on_state_transitioned)
	_states[id] = state

func start(state_id: StringName, msg: Dictionary = {}) -> void:
	if not _states.has(state_id):
		push_error("StateMachine: no state registered with id '%s'" % state_id)
		return
	current_state_id = state_id
	current_state = _states[state_id]
	current_state.enter(msg)

func _process(delta: float) -> void:
	if current_state:
		current_state.update(delta)

func _physics_process(delta: float) -> void:
	if current_state:
		current_state.physics_update(delta)

func _unhandled_input(event: InputEvent) -> void:
	if current_state:
		current_state.handle_input(event)

func transition_to(state_id: StringName, msg: Dictionary = {}) -> void:
	if not _states.has(state_id):
		push_warning("StateMachine: no state registered with id '%s'" % state_id)
		return
	if current_state:
		current_state.exit()
	current_state_id = state_id
	current_state = _states[state_id]
	current_state.enter(msg)

func _on_state_transitioned(state: State, new_state_id: StringName, msg: Dictionary) -> void:
	if state != current_state:
		return # stale signal from a state that already exited
	transition_to(new_state_id, msg)
