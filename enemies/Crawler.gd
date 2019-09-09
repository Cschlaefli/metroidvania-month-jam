extends Enemy

enum directions {left = -1, right = 1, random = 0 }
export(directions) var move_direction = 1
var turning = false
var new_angle = 0
export var speed = Globals.CELL_SIZE * 2

#Typical overrides#

func float_compare(a, b) :
	return abs(a - b) <= .0001

func lerp_angle(from, to, weight):
	return from + short_angle_dist(from, to) * weight

func short_angle_dist(from, to):
	var max_angle = PI * 2
	var difference = fmod(to - from, max_angle)
	return fmod(2 * difference, max_angle) - difference

func _ready():
	randomize()
	if move_direction == directions.random :
		move_direction = sign(rand_range(-1,1))

func _handle_agro(delta):
	if turning :
		velocity = Vector2.ZERO
#		print(curr_enemy.rotation, new_angle)
		var lerp_val = lerp_angle(curr_enemy.rotation, new_angle, delta*20)
		if abs(lerp_val-curr_enemy.rotation) <= .0003 :
			curr_enemy.rotation = new_angle
			turning  = false
		else :
			curr_enemy.rotation = lerp_val
	else :
		var check_rot = $EnemyBody/CrawlerCasts._check(move_direction)
		if check_rot == 0 :
			velocity = lerp(velocity, move_direction * Vector2.RIGHT.rotated(curr_enemy.rotation) * speed, delta*20)
			return
		else :
			new_angle = curr_enemy.rotation + check_rot
			turning = true

func _handle_idle(delta):
	_set_state(states.agro)

func _handle_frozen(delta):
	pass

func _handle_afraid(delta):
	pass

func _handle_hitstun(delta):
	pass

func _handle_casting(delta):
	velocity = lerp(velocity, Vector2.ZERO, delta * 3)

func _handle_recovery(delta):
	pass

func cast():
	pass#set casting_spell and change state to casting


func _on_Hurtbox_hit(body):
	velocity = Vector2.ZERO
	move_direction = -move_direction

#Safe-ish overrides

func _on_CastTimer_timeout():
	casting_spell.cast(self, curr_enemy.global_position, player_dir)
	_set_state(states.recovery)
	casting_spell = null

func hit(by : Node2D, damage : float, type : int, knockback := Vector2.ZERO, hitstun_time := .1):
	hp -= damage
	match type :
		Damage.none :
			velocity = Vector2.ZERO
			move_direction = -move_direction
			hitstun_timer.start(hitstun_time)
			_set_state(states.hitstun)
		Damage.fear :
			fear_timer.start(hitstun_time)
			_set_state(states.afraid)
		Damage.freeze :
			frozen_timer.start(hitstun_time)
			_set_state(states.frozen)
	if hp <= 0:
		die()
		_set_state(states.disabled)

##Rest of script