extends Enemy

export var agro_range := 600

export var speed = 400
export var accel = 4
var direction : Vector2
export var bounce_interval := 10.0
onready var wall_check = $EnemyBody/WallCheck
onready var bounce_check = $EnemyBody/BounceCheck
export var cast_interval := 2.0

var orbit_dir = 1

#export var cast_dist := 500

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
	velocity = lerp(velocity, direction * speed, delta * accel)
	wall_check.cast_to = 1000 * direction
	if $ShootTimer.is_stopped() :
		$ShootTimer.start(cast_interval)
		cast()
	
#	if velocity.length() < 2000.0 && !decelerating:
#		velocity = lerp(velocity, charge_dir, delta * accel * 2)
#	else:
#		decelerating = true
#		velocity = lerp(velocity, Vector2.ZERO, delta / 2)
	

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
	#turn this into a spell so you can change the damage
	casting_spell = $EnemyBody/Charge
	casting_spell.dir = player_dir
	casting_spell.by = self
	_set_state(states.casting)
#	decelerating = false
#	charge_dir = player_dir * Globals.CELL_SIZE * 15


func _on_RecoveryTimer_timeout():
	_set_state(states.agro)
	velocity = Vector2.ZERO