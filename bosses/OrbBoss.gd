extends Boss

export var phase_two_threshold := 15.0
export var phase_three_threshold := 10.0
var active := false

onready var phase_one_shots = $Body/PhaseOneShots
onready var phase_two_shots = $Body/PhaseTwoShots

onready var body = $Body

var teleport_points = []

func _ready():
	teleport_points = $Telepoints.get_children()
	teleport_points.append($Telepoints)
	teleport_points.shuffle()
	add_to_group("persist")
	_add_state("idle")
	_add_state("dead")
	_add_state("phase_one")
	_add_state("phase_two")
	_add_state("phase_three")
	_add_state("teleporting")
	if persist :
		_set_state(states.idle)
	else :
		_set_state(states.dead)

func die() :
	_death_animation()
	.die()

var d_time := 0
var dead = false

func _handle_death(delta):
	d_time += 1
	if not dead and time <= 0 :
		dead = true
		$TeleSpawn.add_child(tele.instance())
		$Body/Sprite.modulate.a = 0
	time -= delta
	$Body/Sprite.material.set_shader_param("amount", sin(time*5*d_time)*2)
	$Body/Sprite.modulate.a = time * .1

func _phase_one_activate() :
	phase_one_shots.activate()

func _phase_two_activate():
	phase_one_shots.deactivate()

func _phase_three_activate() :
	_set_state(states.teleporting)
	phase_one_shots.activate(.5)
#	phase_two_shots.activate(.5)

var tele = preload("res://screens/pickups/TeleportPickup.tscn")

func _death_animation() :
	time = 10
	$Body/Sprite/DeathEffect.emitting = true
	$Body/Hitbox.collision_layer = 0
	phase_one_shots.deactivate()
	phase_two_shots.deactivate()

var time = PI
var tele_delay := 2.0

func _handle_teleport(delta):
	time += delta
	$Body/Sprite.material.set_shader_param("amount", sin(time*20)*2.5)
	if time >= tele_delay :
		_teleport()

var curr_tele = 0

func _teleport() :
	$Body/Sprite.material.set_shader_param("amount", 0)
	if curr_tele >= teleport_points.size() : curr_tele = 0
	var new = teleport_points[curr_tele].position
	curr_tele += 1
	body.position = new
	set_phase()

func _handle_phase_one(delta):
	$Body/Sprite.rotation += PI/8 * delta
	time += delta
	body.position.x -= sin(time * PI/8) * 20
	body.position.y += cos(time) * 10

func _handle_phase_two(delta):
	$Body/Sprite.rotation += PI/8 * delta
	time += delta
	if time >= 5 :
		time = 0
		_set_state(states.teleporting)
		phase_two_shots._fire_all()

func _handle_phase_three(delta):
	$Body/Sprite.rotation += PI/8 * delta
	time += delta
	if time >= 5 :
		time = 0
		phase_two_shots._fire_all()
		_set_state(states.teleporting)


func activate(cam, point):
	if not active :
		cam.transition(position, cam.current_limits, 12)
		_set_state(states.phase_one)
		active = true

func set_phase():
	if health <= 0 :
		die()
	elif state == states.teleporting and time <= tele_delay : return
	elif health <= phase_three_threshold :
		_set_state(states.phase_three)
	elif health <= phase_two_threshold :
		_set_state(states.phase_two)
	else :
		_set_state(states.phase_one)

func hit(by : Node2D, damage : float, type : int, knockback := Vector2.ZERO, hitstun := .3):
	health -= damage
	set_phase()
	$Body/Sprite.modulate = Color.red
	yield(get_tree().create_timer(.1), "timeout")
	$Body/Sprite.modulate = Color.white

func _state_logic(delta):
	match state :
		states.phase_one :
			_handle_phase_one(delta)
		states.phase_two :
			_handle_phase_two(delta)
		states.phase_three :
			_handle_phase_three(delta)
		states.teleporting :
			_handle_teleport(delta)
		states.dead :
			_handle_death(delta)

func _get_transition(delta) :
	match state :
		_:
			pass

func _exit_state(previous_state, new_state):
	match previous_state :
		_:
			pass

func _enter_state(new_state, previous_state):
	if previous_state == states.teleporting : return
	match new_state :
		states.phase_one :
			_phase_one_activate()
		states.phase_two :
			_phase_two_activate()
		states.phase_three :
			_phase_three_activate()
		_:
			pass
