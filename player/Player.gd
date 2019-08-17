extends KinematicBody2D

class_name Player

var state = null setget _set_state
var previous_state = null
var states: Dictionary = {}

var velocity = Vector2.ZERO
var move_speed = 1024
var jump_height = 4068
var gravity = 1000
var facing_direction := 1

onready var cam = $Camera2D

func _ready():
	Screens.player = self
	_add_state('idle')
	_add_state('disabled')
	_set_state(states.idle)

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
	if Input.is_action_pressed('jump') : # && (is_on_floor() or not cayote_timer.is_stopped()) :
		velocity.y = -jump_height

	if is_on_ceiling():
		velocity.y = 5

#
#func _handle_aiming():
#	aim_position = get_local_mouse_position()
#	targeted_position = get_global_mouse_position()
#	gun.rotation = aim_position.angle()
#	gun.position = aim_position.normalized() * 10
#
#	if targeted_position.x < global_position.x:
#		facing_direction = -1
#	else:
#		facing_direction = 1
#

func _input(event: InputEvent):
	if event.is_action_released('jump') && velocity.y < 0:
		velocity.y *= .4
#
#	if event.is_action_pressed('shoot'):
#		if gun.charge_type == Damage.air:
#			velocity = -Vector2.RIGHT.rotated(gun.rotation) * 1200
#		gun.shoot()


func _add_state(state_name):
	states[state_name] = states.size()


func _state_logic(delta : float):
#	$StateLabel.text = states.keys()[state]
	if state != states.disabled:
#		_handle_time(delta)
#		_handle_aiming()
#		_handle_player_direction()
		_handle_gravity()
		_handle_movement(delta)
		_handle_jumping()
		_apply_velocity()

func _handle_gravity():
	if is_on_floor():
		velocity.y = 0
	else:
		velocity.y += gravity

func _apply_velocity():
	move_and_slide(velocity, Vector2.UP)

func _physics_process(delta: float):
	if state != null:
		_state_logic(delta)
		var transition = _get_transition(delta)
		if transition != null:
			_set_state(transition)

func _set_state(new_state):
	previous_state = state
	state = new_state

	if previous_state != null:
		_exit_state(previous_state, new_state)
	if new_state != null:
		_enter_state(new_state, previous_state)

func _get_transition(delta : float):
	pass
#	match state:
#		states.idle:
#			if !is_on_floor():
#				if velocity.y < 0:
#					return states.jump
#				elif velocity.y >= 0.1:
#					cayote_timer.start()
#					return states.fall
#			elif abs(velocity.x) >= 200.0:
#				return states.run
#		states.run:
#			if !is_on_floor():
#				if velocity.y < 0:
#					return states.jump
#				elif velocity.y >= 0.1:
#					cayote_timer.start()
#					return states.fall
#			elif abs(velocity.x) < 200.0:
#				return states.idle
#		states.jump:
#			if is_on_floor():
#				return states.idle
#			elif velocity.y >= 0.1:
#				return states.fall
#		states.fall:
#			if is_on_floor():
#				return states.idle
#			elif velocity.y < 0:
#				return states.jump
#		states.climb:
#			if is_on_floor():
#				return states.idle
#			match _get_nearby_wall():
#				-1:
#					if !Input.is_action_pressed('move_left'):
#						return states.fall
#				1:
#					if !Input.is_action_pressed('move_right'):
#						return states.fall
	return null

func _exit_state(old_state, new_state):
	pass

func _enter_state(new_state, old_state):
	pass
#	match state:
#		states.idle:
#			sprite.play('idle')
#		states.run:
#			sprite.play('run')
#		states.jump:
#			sprite.play('jump')
#		states.climb:
#			sprite.play('jump')