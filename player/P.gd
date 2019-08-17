extends KinematicBody2D
class_name Player

const CELL := 64

export var hp := 5
export var move_speed: float = CELL * 4.5
export var accel_speed: float = CELL * 2
export var jump_height: float = CELL * 17
var detach_x := 10 * CELL

var aim_position := Vector2.ZERO
var targeted_position := Vector2.ZERO
var velocity := Vector2.ZERO
var gravity := CELL / 1.5
var facing_direction := 0

var state = null setget _set_state
var previous_state = null
var states: Dictionary = {}

onready var sprite := $AnimatedSprite
onready var gun := $Gun
onready var stunned_timer := $StunnedTimer
onready var hp_bar := $HealthBar
onready var hp_anims := $HPAnims
onready var left_cast := $Casts/LeftCast
onready var right_cast := $Casts/RightCast

onready var cayote_timer = $CayoteTimer


func _ready():
	Engine.time_scale = 1
	_update_health_bar()
	$Camera2D.limit_left = Global.START_LIMIT_LEFT
	$Camera2D.limit_bottom = Global.LIMIT_BOT
	$Camera2D.limit_top = Global.START_LIMIT_TOP
	$Camera2D.limit_right = Global.LIMIT_RIGHT
	Global.player = self
	_add_state('idle')
	_add_state('run')
	_add_state('jump')
	_add_state('fall')
	_add_state('climb')
	_add_state('disabled')
	_set_state(states.idle)

func _update_health_bar(_time := 1.0):
	var old_hp = hp_bar.value
	hp_anims.interpolate_property(hp_bar,'value',old_hp,hp,_time,Tween.TRANS_CUBIC,Tween.EASE_OUT)
	hp_anims.start()

func _physics_process(delta: float):
	if state != null:
		_state_logic(delta)
		var transition = _get_transition(delta)
		if transition != null:
			_set_state(transition)

func _apply_velocity():
	move_and_slide(velocity, Vector2.UP)

func _handle_gravity():
	if is_on_floor():
		velocity.y = 0
	else:
		velocity.y += gravity


func hit(by : Node2D, damage : int, type : int, knockback : Vector2):
	modulate.a = .5
	set_collision_layer_bit(4, false)
	Engine.time_scale = .4

	$PlayerHit.play()
	if stunned_timer.is_stopped() :
		stunned_timer.start()

		if state == states.climb:
			_set_state(states.fall)

		hp -= damage
		_update_health_bar(.65)
		if hp <= 0 :
			_die()

func _die():
	Engine.time_scale = 1
	Global.game_end(false)


func _handle_movement(delta):
	var h_weight: float = delta * 15

	if Input.is_action_pressed('move_right'):
		velocity.x = lerp(velocity.x, move_speed, h_weight)
		facing_direction = 1
	elif Input.is_action_pressed('move_left'):
		velocity.x = lerp(velocity.x, -move_speed, h_weight)
		facing_direction = -1
	else:
		velocity.x = lerp(velocity.x, 0, h_weight / 2)


func _handle_jumping():
	if Input.is_action_pressed('jump') && (is_on_floor() or not cayote_timer.is_stopped()) :
		velocity.y = -jump_height

	if is_on_ceiling():
		velocity.y = 5


func _handle_aiming():
	aim_position = get_local_mouse_position()
	targeted_position = get_global_mouse_position()
	gun.rotation = aim_position.angle()
	gun.position = aim_position.normalized() * 10

	if targeted_position.x < global_position.x:
		facing_direction = -1
	else:
		facing_direction = 1


func _input(event: InputEvent):
	if event.is_action_released('jump') && velocity.y < 0:
		velocity.y *= .4

	if event.is_action_pressed('shoot'):
		if gun.charge_type == Damage.air:
			velocity = -Vector2.RIGHT.rotated(gun.rotation) * 1200
		gun.shoot()


func _add_state(state_name):
	states[state_name] = states.size()


func _state_logic(delta : float):
	$StateLabel.text = states.keys()[state]
	if state != states.disabled:
		_handle_time(delta)
		_handle_aiming()
		_handle_player_direction()
		if state == states.climb:
			_handle_climbing(delta)
		else:
			_handle_gravity()
			_handle_movement(delta)
			_handle_jumping()
		_apply_velocity()

func _get_nearby_wall():
	#returns -1 for wall on left
	# +1 for wall on right
	# +2 for wall on both sides
	# or 0 for no walls near player
	if right_cast.is_colliding() && !left_cast.is_colliding():
		return 1
	elif left_cast.is_colliding() && !right_cast.is_colliding():
		return -1
	elif right_cast.is_colliding() && left_cast.is_colliding():
		return 2
	else:
		return 0

func _handle_climbing(delta):
	if not $Casts/Downcast.is_colliding():
		velocity.x = 0
	else :
		_handle_movement(delta)


	if Input.is_action_just_pressed('jump'):
		match _get_nearby_wall():
			-1:
				velocity = Vector2(detach_x, -10 * CELL)
			1:
				velocity = Vector2(-detach_x, -10 * CELL)
			0:
				_handle_movement(delta)
				velocity = Vector2(detach_x * facing_direction, -10 * CELL)
		_set_state(states.jump)

	if Input.is_action_pressed('move_up'):
		velocity.y = lerp(velocity.y, -move_speed, delta * 20)
	elif Input.is_action_pressed('move_down'):
		velocity.y = lerp(velocity.y, move_speed, delta * 20)
	else :
		velocity.y = lerp(velocity.y, 0, delta * 20)

func _handle_player_direction():
	if facing_direction == -1:
		sprite.flip_h = true
		gun.base.flip_v = true
		gun.accent.flip_v = true
	elif facing_direction == 1:
		sprite.flip_h = false
		gun.base.flip_v = false
		gun.accent.flip_v = false


func _handle_time(delta):
	if !stunned_timer.is_stopped():
		Engine.time_scale += delta * 2.5
		Engine.time_scale = min(Engine.time_scale, 1)
	elif hp == 1:
		pass
		#if projectiles are nearby then scale time to be slower

func _get_transition(delta : float):
	match state:
		states.idle:
			if !is_on_floor():
				if velocity.y < 0:
					return states.jump
				elif velocity.y >= 0.1:
					cayote_timer.start()
					return states.fall
			elif abs(velocity.x) >= 200.0:
				return states.run
		states.run:
			if !is_on_floor():
				if velocity.y < 0:
					return states.jump
				elif velocity.y >= 0.1:
					cayote_timer.start()
					return states.fall
			elif abs(velocity.x) < 200.0:
				return states.idle
		states.jump:
			if is_on_floor():
				return states.idle
			elif velocity.y >= 0.1:
				return states.fall
		states.fall:
			if is_on_floor():
				return states.idle
			elif velocity.y < 0:
				return states.jump
		states.climb:
			if is_on_floor():
				return states.idle
			match _get_nearby_wall():
				-1:
					if !Input.is_action_pressed('move_left'):
						return states.fall
				1:
					if !Input.is_action_pressed('move_right'):
						return states.fall
	return null


func _enter_state(new_state, old_state):
	match state:
		states.idle:
			sprite.play('idle')
		states.run:
			sprite.play('run')
		states.jump:
			sprite.play('jump')
		states.climb:
			sprite.play('jump')


func _exit_state(old_state, new_state):
	pass


func _set_state(new_state):
	previous_state = state
	state = new_state

	if previous_state != null:
		_exit_state(previous_state, new_state)
	if new_state != null:
		_enter_state(new_state, previous_state)


func _on_StunnedTimer_timeout():
	modulate.a = 1
	set_collision_layer_bit(4, true)
	Engine.time_scale = 1
