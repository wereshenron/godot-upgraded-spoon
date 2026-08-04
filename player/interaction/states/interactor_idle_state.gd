class_name InteractorIdleState
extends State
## Interactor is not holding anything: scan for a Holdable to highlight,
## and let the player pick it up once the cooldown clears.

# var _current_target: Holdable = null
var _pickup_locked_until_msec: int = 0

func enter(message: Dictionary = {}) -> void:
	if message.get("start_cooldown", false):
		_pickup_locked_until_msec = Time.get_ticks_msec() + int(actor.pickup_cooldown * 1000)

func _can_pickup() -> bool:
	return Time.get_ticks_msec() >= _pickup_locked_until_msec
