extends Enemy

export var agro_range := 600

export var speed = 400
export var accel = 4
var direction : Vector2
export var bounce_interval := 10.0
onready var wall_check = $EnemyBody/WallCheck
onready var bounce_check = $EnemyBody/BounceCheck
onready var hurtbox = $EnemyBody/Hurtbox
var cast_interval := 2.0

var orbit_dir = 1

#export var cast_dist := 500

func _ready():
	randomize()
	direction = Vector2.UP.rotated(rand_range(-PI, PI))
	wall_check.cast_to = 500 * direction

func respawn():
	.respawn()
	wall_check = $EnemyBody/WallCheck
	hurtbox = $EnemyBody/Hurtbox
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

	if player_dist <= agro_range :
		_set_state(states.agro)
	else :
		velocity = lerp(velocity, direction * speed, delta * accel)
		wall_check.cast_to = 1000 * direction

func _handle_agro(delta):
	direction = player_dir
	modulate = Color.red
	velocity = lerp(velocity, direction * speed, delta * accel)
	wall_check.cast_to = 1000 * direction
	if $ShootTimer.is_stopped() :
		$ShootTimer.start(cast_interval)
		cast()

func _on_BounceCheck_entered(body):
	_change_direction()

func cast() :
	#turn this into a spell so you can change the damage
	velocity = player_dir * Globals.CELL_SIZE * 15