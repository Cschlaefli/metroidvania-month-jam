extends Spell

onready var projectile := preload('res://projectiles/SimpleProjectile.tscn')

var line_dist = 1000

func _show_guide(delta):
	pass
#	$RayCast2D.cast_to = (get_global_mouse_position() - global_position).normalized() * 500

func _draw():
	var color = Color.black
	
	if not can_cast : color = Color.red
	color.a = .3
	if guide :
		draw_line(position, (get_global_mouse_position() - global_position).normalized() * line_dist, color)
	else :
		pass

func cast(by : Node2D, point : Vector2 ,  direction : Vector2):
	var to_add = projectile.instance()
	to_add.knockback = Vector2(4* Globals.CELL_SIZE, -5 * Globals.CELL_SIZE)
	to_add.hitstun = hitstun
	to_add.collision_mask = hitmask
	to_add.position = point
	to_add.rotation = direction.angle()
	to_add.direction = direction
	projectiles.add_child(to_add)
	.cast(by, point, direction)
