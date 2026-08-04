class_name Holdable
extends Interactable

@export var hold_offset: Vector3
# @export var geometry_list: Array[GeometryInstance3D]
@export var _movement_lower_threshold: float = 0.33
@export var base_follow_speed: float = 15.0
@export var aim_follow_speed: float = 20.0
@export var mass_influence: float = 1.5
@onready var body: RigidBody3D = get_parent()

var _movement: float
var _has_spiked: bool = false
var is_aiming = false

signal grabbed
signal released

func _ready() -> void:
	super._ready()
	# Hook up signals
	grabbed.connect(on_grabbed)
	
	# Assign parent body to Holdable if not already
	# var found := find_children("*", "MeshInstance3D", true, false)
	# _mesh_instances.assign(found)
	body.add_to_group("Holdable")

func _physics_process(_delta: float) -> void:
	_handle_ccd()
	
func on_grabbed() -> void:
	set_highlighted(false)
	SignalBus.looked_away.emit()

func interact(interactor: Interactor, message: Dictionary = {}) -> void:
	var target: Interactable = message.get("target")
	if !target or interactor.state_machine.current_state is InteractorHoldingState:
		return
	interactor.state_machine.transition_to(&"Holding", {"holdable": target})

#### Physics Interactions ####

func set_should_hover(is_hovering: bool) -> void:
	body.axis_lock_linear_y = is_hovering

#### Holdable contract — override in subclasses ####

## Called every physics frame while held, to compute Interactor's follow offset.
## Default: no offset (subclasses like Throwable override to add aim-shift).
func get_hold_offset() -> Vector3:
	return Vector3.ZERO

func get_follow_speed(base_speed: float) -> float:
	return base_speed

@warning_ignore("standalone_expression")
func primary_pressed(_aim_context: Callable = func(): null) -> void:
	pass

@warning_ignore("standalone_expression")
func primary_held(_delta: float, _aim_context: Callable = func(): null) -> void:
	pass

@warning_ignore("standalone_expression")
func primary_released(_aim_context: Callable = func(): null) -> void:
	pass
	
@warning_ignore("standalone_expression")
func secondary_pressed(_aim_context: Callable = func(): null) -> void:
	pass

@warning_ignore("standalone_expression")
func secondary_held(_delta: float, _aim_context: Callable = func(): null) -> void:
	pass

@warning_ignore("standalone_expression")
func secondary_released(_aim_context: Callable = func(): null) -> void:
	pass

func can_use() -> bool:
	return true

func update_hold(_hold_pivot: Node3D, _delta: float) -> void:
	pass

#### Helpers ####

func _handle_ccd() -> void:
	if body.continuous_cd == false:
		_has_spiked = false
		return

	_movement = clampf(body.linear_velocity.length(), 0.0, 1.0)

	if _movement > _movement_lower_threshold:
		_has_spiked = true
	elif _has_spiked:
		body.continuous_cd = false
		_has_spiked = false
