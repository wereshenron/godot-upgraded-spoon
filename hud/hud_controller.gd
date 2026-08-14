extends CanvasLayer

@export var show_timeout: float = 0.12
@export var hide_timeout: float = 0.08

@onready var _instruction_label: HBoxContainer = $Root/HBoxContainer
@onready var _reticle: RichTextLabel = $Root/Cursor
@onready var _bold_cursor: RichTextLabel = $Root/BoldCursor
@onready var _action_label: Label = $Root/HBoxContainer/ActionLabel
@onready var _item_label: Label = $Root/HBoxContainer/ItemLabel

var _tween: Tween
var _target_action: String = ''
var _target_title: String = ''

func _ready() -> void:
	_instruction_label.visible = false
	_bold_cursor.visible = false
	_item_label.visible = false
	
	SignalBus.interactable_seen.connect(_target_seen)
	SignalBus.looked_away.connect(hide_target)
	SignalBus.show_reticle.connect(show_reticle)

func _target_seen(_target) -> void:
	_start_fade(1.0, show_timeout)
	
	_target_title = _target.get("title")
	_target_action = _target.get("action")
		
	_item_label.text = _target_title
	_action_label.text = _target_action
	
	_instruction_label.visible = true
	_bold_cursor.visible = true
	
	if _item_label.text != '':
		_item_label.visible = true
	

func hide_target() -> void:
	_start_fade(0.0, hide_timeout)
	_tween.chain().tween_callback(_on_hide_tween)

func _start_fade(target_alpha: float, duration: float) -> void:
	if _tween:
		_tween.kill()
	_tween = create_tween().set_parallel(true)
	_tween.tween_property(_instruction_label, "modulate:a", target_alpha, duration)
	_tween.tween_property(_bold_cursor, "modulate:a", target_alpha, duration)

func _on_hide_tween() -> void:
	_instruction_label.visible = false
	_bold_cursor.visible = false
	
func show_reticle(state: bool) -> void:
	_reticle.visible = state
