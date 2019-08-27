extends Spell

onready var ice = preload('res://player/projectiles/IceBurst.tscn')

func cast(by : Node2D, point : Vector2 ,  direction : Vector2):
	var pos = get_global_mouse_position()
	var to_add = ice.instance()
	to_add.global_position = pos
	add_child(to_add)
	