extends Node2D
class_name Enemy

var velocity = Vector2.ZERO

export var max_hp := 1.0
export var damage := 1.0
export var gravity = 8
export var terminal_velocity = 5
export var mana_dropped := 1
onready var mana := preload('res://enemies/ManaPellet.tscn')
var hp := 1.0

var dead := false
var ENEMY : PackedScene
onready var curr_enemy : KinematicBody2D = $EnemyBody
onready var fear_timer := $FearTimer

signal die

func _ready() :
	ENEMY = PackedScene.new()
	for child in curr_enemy.get_children() : child.owner = curr_enemy
	ENEMY.pack(curr_enemy)
	hp = max_hp
	add_states()
	_set_state(states.idle)

func hit(by : Node2D, damage : float, type : int, knockback := Vector2.ZERO):
	hp -= damage
	if hp <= 0:
		die()
		_set_state(states.disabled)

func sleep():
	_set_state(states.disabled)

func wake():
	_set_state(states.idle)


func respawn():
	velocity = Vector2.ZERO
	if dead :
		curr_enemy = ENEMY.instance()
		add_child(curr_enemy)
		curr_enemy.connect("hit", self, "hit")

	hp = max_hp
	curr_enemy.position = Vector2.ZERO

	dead = false

func die():
	for i in range(0,mana_dropped):
		var to_add = mana.instance()
		to_add.position = curr_enemy.position
		to_add.velocity = (Vector2.UP * Globals.CELL_SIZE * 3).rotated(rand_range(0,PI * 2))
		call_deferred('add_child', to_add)
	curr_enemy.queue_free()
	dead = true
	curr_enemy = null

#####################
###State Machine ####
#####################

func add_states():
	_add_state("disabled")
	_add_state("idle")
	_add_state("agro")
	_add_state('fear')


func _state_logic(delta : float):
	if dead :
		return

	if not state in [states.disabled]  :
		if state == states.agro :
			_handle_agro(delta)
		elif state == states.idle :
			_handle_idle(delta)
		elif state == states.fear:
			_handle_fear(delta)
		_handle_gravity(delta)
		_apply_movement(delta)

func _handle_agro(delta):
	pass

func _handle_idle(delta):
	pass

func _handle_fear(delta):
	print('afraid')

func _apply_movement(delta):
	curr_enemy.move_and_slide(velocity, Vector2.UP)

func _handle_gravity(delta) :
	if not curr_enemy.is_on_floor() :
		velocity.y += gravity * delta * Globals.CELL_SIZE
		velocity.y = min(velocity.y , terminal_velocity * Globals.CELL_SIZE)


var state = null setget _set_state
var previous_state = null
var states: Dictionary = {}

func _add_state(state_name):
	states[state_name] = states.size()

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

func _exit_state(old_state, new_state):
	pass

func _enter_state(new_state, old_state):
	match new_state:
		states.fear:
			fear_timer.start()

########################################
####State transitions
#######################################



func _on_FearTimer_timeout():
	_set_state(states.idle)
