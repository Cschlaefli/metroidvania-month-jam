extends Spell

onready var ice = preload('res://projectiles/IceBurstProjectile.tscn')

func cast(by : Node2D, point : Vector2 ,  direction : Vector2):
	var pos = get_global_mouse_position()
	var to_add = ice.instance()
	to_add.collision_mask = hitmask
	to_add.global_position = pos
	projectiles.add_child(to_add)
	.cast(by, point, direction)