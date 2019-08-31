extends Boss

func _ready():
	add_to_group("persist")
	_add_state("idle")
	_add_state("dead")
	_add_state("phase_one")
	_add_state("phase_two")
	_add_state("phase_three")
	if persist :
		_set_state(states.idle)
	else :
		_set_state(states.dead)

func _handle_phase_one(delta):
	pass

func _handle_phase_two(delta):
	pass

func _handle_phase_three(delta):
	pass

func activate():
	_set_state(states.phase_one)

func hit(by : Node2D, damage : float, type : int, knockback := Vector2.ZERO, hitstun := .3):
	health -= damage
	if health <= 0 :
		die()
	pass

func _state_logic(delta):
	match state :
		states.phase_one :
			_handle_phase_one(delta)
		states.phase_two :
			_handle_phase_two(delta)
		states.phase_three :
			_handle_phase_three(delta)

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