extends Enemy

export var agro_range := 600

export var speed = 400
export var accel = 4
var direction : Vector2

func respawn():
	.respawn()

func _change_direction(bod := 0):
	randomize()
	if state in [states.disabled] :
		return
	elif state == states.agro :
		pass

func _handle_idle(delta):
	if Globals.player.global_position.distance_to(curr_enemy.global_position) <= agro_range :
		_set_state(states.agro)
	else :
		pass

func _handle_agro(delta):
	direction = Globals.player.global_position - global_position
	velocity = lerp(velocity, direction * speed, delta * accel)