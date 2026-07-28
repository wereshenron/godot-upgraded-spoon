class_name InteractorIdleState
extends State
## Interactor is not holding anything: scan for a Holdable to highlight,
## and let the player pick it up once the cooldown clears.

var _current_target: Holdable = null
var _pickup_locked_until_msec: int = 0

func enter(message: Dictionary = {}) -> void:
	if message.get("start_cooldown", false):
		_pickup_locked_until_msec = Time.get_ticks_msec() + int(actor.pickup_cooldown * 1000)

func exit() -> void:
	if _current_target:
		_current_target.set_highlighted(false)
		_current_target = null

func update(_delta: float) -> void:
	var new_target: Holdable = null

	if actor.pickup_raycast.is_colliding() and _can_pickup():
		var body = actor.pickup_raycast.get_collider()
		if body.is_in_group("Holdable"):
			new_target = actor.get_holdable(body)

	if new_target == _current_target:
		return

	if _current_target:
		_current_target.set_highlighted(false)

	_current_target = new_target

	if _current_target:
		_current_target.set_highlighted(true)
		SignalBus.interactable_seen.emit(_current_target)
	else:
		SignalBus.looked_away.emit()

func handle_input(event: InputEvent) -> void:
	if event.is_action_pressed("pick_up") and _current_target and _can_pickup():
		transitioned.emit(self, &"Holding", {"holdable": _current_target})

func _can_pickup() -> bool:
	return Time.get_ticks_msec() >= _pickup_locked_until_msec
