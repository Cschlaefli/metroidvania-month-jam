extends Node2D
class_name Enemy

var velocity = Vector2.ZERO

export var max_hp := 1.0
export var gravity = 8
export var terminal_velocity = 5.0
export var mana_dropped := 1
export var mana_value := 5.0
onready var mana := preload('res://enemies/ManaPellet.tscn')
export var shoot_rand := .1
export var shoot_interval := 2.0
var hp := max_hp

var dead := false
var ENEMY : PackedScene
onready var curr_enemy : KinematicBody2D = $EnemyBody
onready var hurtbox := $EnemyBody/Hurtbox
onready var fear_timer := $FearTimer
onready var hitstun_timer := $HitstunTimer
onready var casting_timer := $CastTimer
onready var recovery_timer := $RecoveryTimer
onready var shoot_timer := $ShootTimer
onready var frozen_timer := $FrozenTimer
onready var line_of_sight := $EnemyBody/LineOfSight

var player_dist := 1.0
var player_dir := Vector2.ZERO
var sees_player := false

var casting_spell : Spell

signal die


#Typical overrides#

func _handle_agro(delta):
	pass

func _handle_idle(delta):
	pass

func _handle_frozen(delta):
	pass

func _handle_afraid(delta):
	pass

func _handle_hitstun(delta):
	pass

func _handle_casting(delta):
	velocity = lerp(velocity, Vector2.ZERO, delta * 3)

func _handle_recovery(delta):
	pass

func cast():
	pass#set casting_spell and change state to casting


func _on_Hurtbox_hit(body):
	pass # Replace with function body.

#Safe-ish overrides

func _on_CastTimer_timeout():
	casting_spell.cast(self, curr_enemy.global_position, player_dir)
	_set_state(states.recovery)
	casting_spell = null

func hit(by : Node2D, damage : float, type : int, knockback := Vector2.ZERO, hitstun_time := .1):
	hp -= damage
	match type :
		Damage.none :
			hitstun_timer.start(hitstun_time)
			_set_state(states.hitstun)
		Damage.fear :
			fear_timer.start(hitstun_time)
			_set_state(states.afraid)
		Damage.freeze :
			frozen_timer.start(hitstun_time)
			_set_state(states.frozen)
	if hp <= 0:
		die()
		_set_state(states.disabled)

##Rest of script

func _ready() :
	ENEMY = PackedScene.new()
	for child in curr_enemy.get_children() :
		for c in child.get_children():
			c.owner = curr_enemy
		child.owner = curr_enemy
	ENEMY.pack(curr_enemy)
	hp = max_hp
	curr_enemy.connect("hit", self, "hit")
	add_states()
	shoot_interval += rand_range(-shoot_rand, shoot_rand)
	_set_state(states.disabled)


#State setting#

func sleep():
	_set_state(states.disabled)

func wake():
	_set_state(states.wake)

func respawn():
	velocity = Vector2.ZERO
	if dead :
		curr_enemy = ENEMY.instance()
		add_child(curr_enemy)
		hurtbox = $EnemyBody/Hurtbox
		line_of_sight = $EnemyBody/LineOfSight
		curr_enemy.connect("hit", self, "hit")

	hp = max_hp
	curr_enemy.position = Vector2.ZERO
	dead = false

func die():
	for i in range(0,mana_dropped):
		var to_add = mana.instance()
		to_add.amount = mana_value
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
	_add_state("hitstun")
	_add_state("idle")
	_add_state("agro")
	_add_state("wake")
	_add_state("afraid")
	_add_state("frozen")
	_add_state("casting")
	_add_state("recovery")


func _state_logic(delta : float):
	if dead :
		return

	if not state in [states.disabled]  :
		_update_player_pos(delta)
		match state :
			states.agro :
				_handle_agro(delta)
			states.idle :
				_handle_idle(delta)
			states.afraid:
				_handle_afraid(delta)
			states.frozen :
				_handle_frozen(delta)
			states.casting :
				_handle_casting(delta)
			states.recovery :
				_handle_casting(delta)
			states.hitstun :
				_handle_hitstun(delta)
		_handle_gravity(delta)
		_apply_movement(delta)

func _update_player_pos(delta):
	var temp = Globals.player.global_position - curr_enemy.global_position
	player_dist = temp.length()
	player_dir = temp.normalized()
	line_of_sight.cast_to = temp

	sees_player = not line_of_sight.is_colliding()

func _apply_movement(delta):
	var pos = curr_enemy.global_position + (velocity * delta * 2)
	var cam = Globals.player.cam
	if not cam is Camera2D : return
	if not (pos.x < cam.limit_right and pos.x > cam.limit_left and pos.y > cam.limit_top and pos.y < cam.limit_bottom) :
		velocity = - velocity
	curr_enemy.move_and_slide(velocity, Vector2.UP)

func _handle_gravity(delta) :
	if not curr_enemy.is_on_floor() :
		velocity.y += gravity * delta * Globals.CELL_SIZE
		velocity.y = min(velocity.y , terminal_velocity * Globals.CELL_SIZE)


####State logistics###

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

#Transitions#

func _get_transition(delta : float):
	match state :
		states.wake :
			state = states.idle
		states.recovery :
			if recovery_timer.is_stopped() :
				_set_state(states.agro)
		states.afraid :
			if fear_timer.is_stopped() :
				_set_state(states.idle)
		states.frozen :
			if frozen_timer.is_stopped() :
				_set_state(states.agro)
		states.hitstun :
			if hitstun_timer.is_stopped():
				_set_state(states.agro)

func _exit_state(old_state, new_state):
	match old_state :
		states.disabled :
			if new_state != states.wake :
				state = states.disabled
		states.hitstun :
			hurtbox.collision_layer = 2
			hitstun_timer.stop()
			modulate.a  = 1.0
			if curr_enemy :
				curr_enemy.collision_layer = 8
		states.frozen :
			pass
		states.afraid :
			pass
		states.casting :
			if casting_spell and new_state != states.recovery :
				if casting_spell.interuptable :
					casting_spell.interupt()
					casting_spell = null
				elif not state in [states.disabled] :
					state = states.casting
					return
			casting_timer.stop()

func _enter_state(new_state, old_state):
	match new_state:
		states.frozen :
			pass #frozen effect
		states.afraid:
			pass #fear effect
		states.hitstun :
			hurtbox.collision_layer =0
			modulate.a = .5
			curr_enemy.collision_layer = 0
		states.casting :
			casting_spell.start_casting()
			casting_timer.start(casting_spell.casting_time)
		states.recovery :
			recovery_timer.start(casting_spell.recovery_time)
########################################
####State transitions
#######################################




