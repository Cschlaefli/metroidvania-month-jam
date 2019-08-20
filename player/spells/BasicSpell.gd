extends Node

onready var projectile := preload('res://player/SimpleProjectile.tscn')


func cast(point : Vector2 ,  direction : Vector2):
	var to_add = projectile.instance()

	to_add.position = point
	to_add.rotation = direction.angle()
	to_add.direction = direction
	add_child(to_add)
