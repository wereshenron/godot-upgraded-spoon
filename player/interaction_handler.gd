extends Node3D

@export var raycast : RayCast3D
@export var camera : Camera3D

var _current_target: Node3D = null
var _object_held: Node3D = null
var _object_held_orig_parent: Node3D = null

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
				
				
func _unhandled_input(event):
	if event is InputEventMouseButton and event.is_pressed() and _current_target:
		_pickup(_current_target)
	
	if event is InputEventMouseButton and !event.is_pressed() and _object_held:
		_let_go()
	
func _clear_highlight() -> void:
	if _current_target:
		var grabbable = _current_target.get_node_or_null("Grabbable")
		if grabbable and grabbable.has_method("set_highlighted"):
			grabbable.set_highlighted(false) 
		
		
func _set_highlight(target: Node3D, active: bool) -> void:
	var grabbable = target.get_node_or_null("Grabbable")
	if grabbable and grabbable.has_method("set_highlighted"):
		grabbable.set_highlighted(active)
		
func _pickup(target: Node3D) -> void:
	if !target or _object_held:
		return
		
	if target.has_method("set_freeze_enabled"):
		target.set_freeze_enabled(true)
	else: return
	_object_held = target
	_object_held_orig_parent = target.get_parent()
	target.reparent(camera)
	
func _let_go() -> void:
	if !_object_held:
		return
	
	if _object_held.has_method("set_freeze_enabled"):
		_object_held.set_freeze_enabled(false)
	else: return
	_object_held.reparent(_object_held_orig_parent)
	_object_held = null
	return
	
