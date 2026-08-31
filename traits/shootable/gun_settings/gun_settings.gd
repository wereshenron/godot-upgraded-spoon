class_name GunSettings
extends Resource

@export_category("Firing")
## Max recoil applied. x: up/down, y: side-side, z: forward/back
@export var recoil_amount: Vector3
## min recoil applied. same scheme as recoil_amount.
@export var min_recoil_amount: Vector3
@export var max_recoil_offset: Vector3 = Vector3(0.3, 0.3, 0.1)
@export var snap: float
@export var recoil_speed: float
@export var fire_rate: float
@export var fire_velocity: float = 200.0
@export var reference_bullet_mass: float = 1.0
@export var full_auto: bool = false
@export var bullet_scene: PackedScene
@export var damage: float = 20.0
