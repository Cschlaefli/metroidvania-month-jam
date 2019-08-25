extends Enemy

export var agro_range := 600

export var speed = 400
export var accel = 4
var direction : Vector2
export var bounce_interval := 10.0
onready var wall_check = $EnemyBody/WallCheck
var cast_interval := 2.0

var orbit_dir = 1

#export var cast_dist := 500

func _ready():
	randomize()
#	cast_dist += rand_range(-150, 150)
#	orbit_dir = -1 if randf() > .5 else 1
	direction = Vector2.UP.rotated(rand_range(-PI, PI))
	wall_check.cast_to = 500 * direction

func respawn():
	.respawn()
	wall_check = $EnemyBody/WallCheck

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
	if Globals.player.global_position.distance_to(curr_enemy.global_position) <= agro_range :
		_set_state(states.agro)
	else :
		velocity = lerp(velocity, direction * speed, delta * accel)
		wall_check.cast_to = 1000 * direction

var time = 0

func _handle_agro(delta):

	velocity = lerp(velocity, direction * speed, delta * accel)
	wall_check.cast_to = 1000 * direction
	if $ShootTimer.is_stopped() :
		$ShootTimer.start(cast_interval)
		cast()
#	time += delta
#	var dist = (Globals.player.global_position - curr_enemy.global_position)
#	cast_dist += sin(time)*100
#	if dist.length() <=  cast_dist :
#		direction = dist.normalized().rotated(PI/2 * orbit_dir)
#	else :
#		direction = dist.normalized()
#	velocity = lerp(velocity, direction * speed * 1.5, delta * accel)

func _on_BounceCheck_entered(body):
	_change_direction()

func cast() :
	casting_spell = $BasicSpell
	_set_state(states.casting)
