extends Node3D

@export var raycast : RayCast3D

var _current_target: Node3D = null

func _ready() -> void:
	raycast.add_exception(owner)

func _process(_delta: float) -> void:
	if raycast and raycast.is_colliding():
		var target: Node3D = raycast.get_collider()
		if target.is_in_group("Grabbable"):
			if target != _current_target:
				_set_highlight(target, true)
				_current_target = target
			return
			
	_clear_highlight()
	_current_target = null
				
				
func _clear_highlight() -> void:
	if _current_target:
		var grabbable = _current_target.get_node_or_null("Grabbable")
		if grabbable and grabbable.has_method("set_highlighted"):
			grabbable.set_highlighted(false) 
		
		
func _set_highlight(target: Node3D, active: bool) -> void:
	var grabbable = target.get_node_or_null("Grabbable")
	if grabbable and grabbable.has_method("set_highlighted"):
		grabbable.set_highlighted(active)
