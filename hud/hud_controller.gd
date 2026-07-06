extends Control

@onready var instruction_label = $HBoxContainer

func _ready() -> void:
	instruction_label.visible = false
	SignalBus.interactable_seen.connect(_target_seen)
	SignalBus.looked_away.connect(hide_target)

func _target_seen(_target) -> void:
	instruction_label.visible = true

func hide_target() -> void:
	instruction_label.visible = false
