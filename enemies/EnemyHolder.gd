extends Node2D

class_name EnemyHolder

export(PackedScene) var enemy
var curr_enemy : Enemy
var dead := true

func _ready():
	respawn()

func on_death():
	dead = true
	curr_enemy = null

func respawn():
	if dead :
		curr_enemy = enemy.instance()
		add_child(curr_enemy)
		curr_enemy.connect("die", self, "on_death")
	else :
		curr_enemy.hp = curr_enemy.max_hp
		curr_enemy.position = Vector2.ZERO
	dead = false


func wake():
	curr_enemy.set_physics_process(true)
	curr_enemy.set_process_input(true)
	curr_enemy.set_process_internal(true)
	curr_enemy.set_process_unhandled_input(true)
	curr_enemy.set_process_unhandled_key_input(true)


func sleep():
	curr_enemy.set_physics_process(false)
	curr_enemy.set_process_input(false)
	curr_enemy.set_process_internal(false)
	curr_enemy.set_process_unhandled_input(false)
	curr_enemy.set_process_unhandled_key_input(false)

