extends Enemy

export var agro_range := 1000

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
#	cast_dist += rand_range(-150, 150)
#	orbit_dir = -1 if randf() > .5 else 1
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
		return
#		add attacking behavior here

	$BounceTimer.start(bounce_interval + rand_range(-1, 1))
	if wall_check.is_colliding() :
		direction = direction.bounce(wall_check.get_collision_normal())
	else :
		direction = Vector2.UP.rotated(rand_range(-PI, PI))
	wall_check.cast_to = 1000 * direction

func _handle_idle(delta):
	if not( bounce_check.get_overlapping_areas().empty() or bounce_check.get_overlapping_bodies().empty()) :
		_change_direction()

	if player_dist <= agro_range && sees_player:
		shoot_timer.start(shoot_interval)
		_set_state(states.agro)
	else :
		velocity = lerp(velocity, direction * speed, delta * accel)
		wall_check.cast_to = 1000 * direction

var time = 0

func _handle_agro(delta):
	direction = player_dir

	if player_dist <= cast_dist :
		velocity = lerp(velocity, Vector2.ZERO, delta * accel)
	else :
		velocity = lerp(velocity, direction * speed, delta * accel)
	if wall_check != null :
		wall_check.cast_to = 1000 * direction
		if shoot_timer.is_stopped() :
			shoot_timer.start(shoot_interval)
			cast()


func _on_BounceCheck_entered(body):
	_change_direction()

func cast() :
	casting_spell = $EnemyBody/BasicSpell
	_set_state(states.casting)
