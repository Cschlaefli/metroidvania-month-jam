extends Node2D

onready var projectile_spawn_pos := $ProjectilePosition
onready var projectile := preload('res://player/Projectile.tscn')
onready var active_projectile_list := $ActiveProjectiles
onready var cooldown := $CooldownTimer

func shoot():
	if cooldown.is_stopped():
		var to_add = projectile.instance()
		to_add.global_position = projectile_spawn_pos.global_position
		to_add.rotation = rotation - PI / 2
		to_add.direction = (get_global_mouse_position() - projectile_spawn_pos.global_position).normalized()
		to_add.speed = 30
		active_projectile_list.add_child(to_add)
		
		cooldown.wait_time = to_add.projectile_cd
		cooldown.start()