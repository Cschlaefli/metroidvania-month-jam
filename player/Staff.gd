extends Node2D

onready var projectile_spawn_pos := $ProjectilePosition
onready var projectiles := [preload('res://projectiles/SimpleProjectile.tscn'), preload('res://projectiles/FearProjectile.tscn')]
onready var active_projectile_list := $ActiveProjectiles
onready var cooldown := $CooldownTimer
onready var casting_timer := $CastingTimer
onready var selected_projectile = projectiles[0]

func shoot():
#	if cooldown.is_stopped() && casting_timer.is_stopped():
	var to_add = selected_projectile.instance()
#	casting_timer.wait_time = to_add.cast_time
#	casting_timer.start()
#	yield(casting_timer,'timeout')

	to_add.global_position = projectile_spawn_pos.global_position
	to_add.rotation = rotation - PI / 2
	to_add.direction = (get_global_mouse_position() - projectile_spawn_pos.global_position).normalized()
	active_projectile_list.add_child(to_add)

	cooldown.wait_time = to_add.cd
	cooldown.start()
