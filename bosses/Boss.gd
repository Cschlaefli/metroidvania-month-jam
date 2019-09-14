extends Node2D
class_name Boss


export var health := 10.0
export var persist := true

func set_persist(p):
	if not p :
		_set_state(states.dead)

func die():
	persist = false
	_set_state(states.dead)

func activate(cam, point):
	pass

func hit(by : Node2D, damage : float, type : int, knockback := Vector2.ZERO, hitstun := .3):
	health -= damage
	if health <= 0 :
		die()
	pass

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

func _state_logic(delta):
	pass

func _get_transition(delta) :
	match state :
		_:
			pass

func _exit_state(previous_state, new_state):
	match previous_state :
		_:
			pass

func _enter_state(new_state, previous_state):
	match new_state :
		_:
			pass