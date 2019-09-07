extends Enemy

export var agro_range := 600

export var speed = 400
export var accel = 4
var direction : Vector2
export var bounce_interval := 10.0
onready var wall_check = $EnemyBody/WallCheck
onready var bounce_check = $EnemyBody/BounceCheck

var orbit_dir = 1

export var cast_dist := 500

func _ready():
	randomize()
	direction = Vector2.UP.rotated(rand_range(-PI, PI))
	wall_check.cast_to = 500 * direction

func respawn():
	.respawn()
	wall_check = $EnemyBody/WallCheck
	bounce_check = $EnemyBody/BounceCheck

func _change_direction(bod := 0):
	randomize()
	if state in [states.disabled] :
		return
	elif state == states.agro :
		pass
#		add attacking behavior here

	$BounceTimer.start(bounce_interval + rand_range(-1, 1))
	if wall_check.is_colliding() :
		direction = direction.bounce(wall_check.get_collision_normal())
	else :
		direction = Vector2.UP.rotated(rand_range(-PI, PI))
	wall_check.cast_to = 1000 * direction

func _handle_idle(delta):
	modulate = Color.white
	if not( bounce_check.get_overlapping_areas().empty() or bounce_check.get_overlapping_bodies().empty()) :
		_change_direction()

	if player_dist <= agro_range && sees_player:
		cast()
		_set_state(states.agro)
	else :
		velocity = lerp(velocity, direction * speed, delta * accel)
		wall_check.cast_to = 1000 * direction


func _enter_state(new_state, old_state):
	._enter_state(new_state, old_state)
	match new_state:
		states.agro:
			decelerating = false


func _handle_agro(delta):
	direction = player_dir
	modulate = Color.red
	if player_dist <= cast_dist :
		velocity = lerp(velocity, Vector2.ZERO, delta * accel)
	else :
		velocity = lerp(velocity, direction * speed, delta * accel)
	wall_check.cast_to = 1000 * direction
	if shoot_timer.is_stopped() :
		shoot_timer.start(shoot_interval)
		cast()

func _handle_casting(delta):
	pass

func _handle_recovery(delta) :
	velocity = lerp(velocity, velocity.normalized()*200, delta * .5)

func _on_Hurtbox_hit(body):
	pass

var decelerating := false
var charge_dir : Vector2

func _on_BounceCheck_entered(body):
	_change_direction()


func cast() :
	casting_spell = $EnemyBody/Charge
	casting_spell.dir = player_dir
	casting_spell.by = self
	_set_state(states.casting)



func _on_RecoveryTimer_timeout():
	_set_state(states.agro)
	velocity = Vector2.ZERO