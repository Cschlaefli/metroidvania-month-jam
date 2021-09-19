extends Spell

onready var projectile := preload('res://projectiles/FearProjectile.tscn')



func cast(by : Node2D, point : Vector2 ,  direction : Vector2):
	var to_add = projectile.instance()

	to_add.position = point
	to_add.rotation = direction.angle()
	to_add.direction = direction

	projectiles.add_child(to_add)
	.cast(by, point, direction)
