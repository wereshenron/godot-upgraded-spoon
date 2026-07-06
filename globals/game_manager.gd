extends Node

@onready var hud = preload("res://hud/hud.tscn")

func _ready() -> void:
	var _hud_instance = hud.instantiate()
	add_child(_hud_instance)
