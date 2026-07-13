extends CanvasLayer

@export var show_timeout: float = 0.12
@export var hide_timeout: float = 0.08

@onready var instruction_label: HBoxContainer = $Root/HBoxContainer

var _tween: Tween

func _ready() -> void:
	instruction_label.visible = false
	SignalBus.interactable_seen.connect(_target_seen)
	SignalBus.looked_away.connect(hide_target)
	print(get_viewport().get_visible_rect())

func _target_seen(_target) -> void:
	if _tween: _tween.kill()
	
	instruction_label.visible = true
	_tween = create_tween()
	_tween.tween_property(instruction_label, "modulate:a", 1.0, show_timeout)

func hide_target() -> void:
	if _tween: _tween.kill()
	
	_tween = create_tween()
	_tween.tween_property(instruction_label, "modulate:a", 0.0, hide_timeout)
	_tween.tween_callback(func(): instruction_label.visible = false)
