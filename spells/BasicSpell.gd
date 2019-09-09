extends Spell
onready var projectile := preload('res://projectiles/SimpleProjectile.tscn')

var line_dist = 1000
onready var guide_line := $GuideLine

func _show_guide(delta):
	if not guide :
		guide_line.visible = false
		return
	guide_line.visible = true
	var color = Color.black
	if not can_cast : color = Color.red
	color.a = .75
	guide_line.clear_points()
	guide_line.add_point(position)
	var dest = Globals.player.cast_dir * line_dist + position
	guide_line.add_point(dest)
	guide_line.default_color = color

func cast(by : Node2D, point : Vector2 ,  direction : Vector2):
	var to_add = projectile.instance()
	to_add.damage = projectile_damage
	to_add.speed = projectile_speed
	to_add.knockback = Vector2(4* Globals.CELL_SIZE, -5 * Globals.CELL_SIZE)
	to_add.hitstun = hitstun
	to_add.collision_mask = hitmask
	to_add.position = point
	to_add.rotation = direction.angle()
	to_add.direction = direction.normalized()
	projectiles.add_child(to_add)
	.cast(by, point, direction)
