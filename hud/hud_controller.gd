extends CanvasLayer

@onready var instruction_label = $Root/HBoxContainer

func _ready() -> void:
	instruction_label.visible = false
	SignalBus.interactable_seen.connect(_target_seen)
	SignalBus.looked_away.connect(hide_target)
	print(get_viewport().get_visible_rect())

func _target_seen(_target) -> void:
	instruction_label.visible = true

func hide_target() -> void:
	instruction_label.visible = false
