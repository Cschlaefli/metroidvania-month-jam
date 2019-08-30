extends KinematicBody2D

class_name Player


var velocity = Vector2.ZERO

var health := 10.0
var max_health := 10.0

var mana := 15.0
var max_mana := 10.0

var mana_regen_rate := 1.0
var mana_decay_rate := 1.5
var excess_mana := .0

var move_speed = Globals.CELL_SIZE * 8
var default_move_speed = Globals.CELL_SIZE * 8
var run_speed = Globals.CELL_SIZE * 16
export var run_known := false
export var heal_known := false
var jump_height = 4
var double_jump_height = 4
var gravity = Globals.CELL_SIZE * 40
var player_acceleration := 15.0
var player_deceleration := 30
var terminal_velocity = Globals.CELL_SIZE * 15
const TERMINAL_VELOCITY = 256 * 15

#if < 0 infinite jumps, if == 0  no double jumps, if > 0 that many air jumps
var default_jumps := 0

var jumps = default_jumps
var jump_cost := 3.0

var run_cost := 3.3
var facing_direction := 1

onready var staff := $Staff
onready var cam = $Camera2D
onready var cayote_timer = $CayoteTimer
onready var casting_timer = $CastingTimer
onready var recovery_timer = $RecoveryTimer

onready var spell_list = $SpellList
var equipped_spells =  []
var current_spell : Spell
var casting_spell : Spell

onready var spell_equip_menu = $CanvasLayer/SpellEquipMenu

signal spell_list_changed(equipped_spells)

signal resources_changed(health, max_health, mana, max_mana, excess_mana)

func _ready():
	Globals.player = self
	for spell in Spells.SPELL_LIST :
		var to_add = spell.instance() as Spell
		#implement loading the spell knowledge from a save file here
		#and equiped spells
		spell_list.add_child(to_add)
		to_add.connect("updated", self, "_update_spells")

	spell_equip_menu.spells = spell_list.get_children()
	spell_equip_menu.update_display()

	_update_spells()
	if equipped_spells.size() > 0 :
		current_spell = equipped_spells[0]

	_update_resources()
	_add_state('idle')
	_add_state('run')
	_add_state('jump')
	_add_state('fall')
	_add_state('casting')
	_add_state('recovering')
	_add_state('hitstun')
	_add_state('disabled')
	_set_state(states.idle)

func _input(event: InputEvent):
	if event.is_action_released('jump') && velocity.y < 0:
		velocity.y *= .3

	if event.is_action_pressed("shield") and $Shield.known and state != states.recovering :
		if mana >= $Shield.casting_cost :
			if casting_spell :
				if casting_spell.interuptable :
					casting_spell.interupt()
				else : 
					return
			casting_spell = $Shield
			_end_cast()

	
	if event.is_action_pressed("spell_cycle_forward") :
		_cycle_spells()
	if event.is_action_pressed("spell_cycle_back") :
		_cycle_spells(false)
	
	if not state == states.casting and not state == states.recovering :
		if event.is_action_pressed("shoot") :
			if current_spell :
				current_spell.guide = true
				if current_spell.casting_cost <= mana :
					pass
					##some method of showing the mana cost
				else :
					pass
					##some method of showing that mana is low
		if event.is_action_released('shoot'):
			if current_spell :
				if current_spell.casting_cost <= mana :
					casting_spell = current_spell
					_set_state(states.casting)
		if event.is_action_pressed('teleport') and $Teleport.known:
			var spell = $Teleport
			if spell.casting_cost <= mana :
				casting_spell = spell
				casting_spell.direction = facing_direction
				_set_state(states.casting)

func _cycle_spells(forward := true) :
	if forward :
		equipped_spells.push_front(equipped_spells.pop_back())
	else :
		equipped_spells.append(equipped_spells.pop_front())
	current_spell = equipped_spells[0]
	emit_signal("spell_list_changed", equipped_spells)

func _update_spells():
	equipped_spells = []
	for spell in spell_list.get_children() :
		if spell.equipped : equipped_spells.append(spell)
	if equipped_spells.size() > 0 :
		current_spell = equipped_spells[0]
	else :
		current_spell = null
	emit_signal("spell_list_changed", equipped_spells)

func hit(by : Node2D, damage : float, type : int, knockback := Vector2.ZERO, hitstun := .3):
	if state == states.hitstun :
		return
	health -= damage
	var diff = global_position - by.global_position
	diff = Vector2(sign(diff.x), 1)

	velocity = knockback * diff
	_set_state(states.hitstun)
	$HitstunTimer.start(hitstun)
	if health <= 0 :
		die()

func die():
	Globals.player_death()

###############################
###State logic##

func _state_logic(delta : float):
	$StateLabel.text = states.keys()[state]
	_handle_gravity(delta)
	_handle_weapon(delta)
	if state == states.casting :
		_cast_arrest(delta)
	elif not state in [states.disabled, states.hitstun]:
		_handle_movement(delta)
		_handle_jumping()
		_regen_mana(delta)

	_apply_velocity()
	_update_resources()

func _cast_arrest(delta):
	velocity.x = lerp(velocity.x, 0, delta * 5)
#	velocity.y = lerp(velocity.y, 0, delta )

func _handle_gravity(delta):
	if is_on_ceiling():
		velocity.y += Globals.CELL_SIZE

	if is_on_floor():
		if not state == states.hitstun :
			velocity.y = 0
		jumps = default_jumps
	elif velocity.y <= terminal_velocity:
		velocity.y += gravity*delta
	else :
		velocity.y -= gravity*delta * .5

func _handle_movement(delta):

	if run_known and Input.is_action_pressed("run") and mana >= run_cost * delta :
		mana -= run_cost*delta
		move_speed = run_speed
		if not $RunEffect.emitting : $RunEffect.emitting = true
	else :
		$RunEffect.emitting = false
		move_speed = default_move_speed

	if Input.is_action_pressed('move_right'):
		velocity.x = lerp(velocity.x, move_speed, delta * player_acceleration)
		facing_direction = 1
	elif Input.is_action_pressed('move_left'):
		velocity.x = lerp(velocity.x, -move_speed, delta * player_acceleration)
		facing_direction = -1
	else:
		_decel(delta)

func _decel(delta):
	velocity.x = lerp(velocity.x, 0, delta * player_deceleration)

func _handle_weapon(delta):
	if current_spell and current_spell.guide :
		current_spell.can_cast = current_spell.casting_cost <= mana
		if not Input.is_action_pressed("shoot") : current_spell.guide = false
	var point = (get_global_mouse_position() - global_position).angle() + PI / 2
	if state == states.casting :
		staff.rotation = lerp_angle(staff.rotation, point, delta *.5)
	else :
		staff.rotation = lerp_angle(staff.rotation, point, delta * 10 )

func _handle_jumping():
	if Input.is_action_just_pressed('jump') :
		if state in [states.idle, states.run] or not cayote_timer.is_stopped() :
			velocity.y = -sqrt(2*gravity*jump_height * Globals.CELL_SIZE)
		elif jumps != 0  and mana >= jump_cost:
			$DoubleJumpEffect.emitting = true
			$DoubleJumpEffect.restart()
			mana -= jump_cost
			velocity.y = -sqrt(2*gravity*double_jump_height * Globals.CELL_SIZE)
			jumps -= 1

func _apply_velocity():
	move_and_slide(velocity, Vector2.UP)

func _regen_mana(delta):
	var mana_regen = mana_regen_rate
	if state == states.idle and $IdleTimer.is_stopped() :
		mana_regen *= 3
		$RegenParticles.emitting = true
		if mana >= 9.95:
			$RegenParticles.emitting = false
	else :
		$RegenParticles.emitting = false

	if mana < max_mana :
		mana += mana_regen * delta
		excess_mana = 0
	elif mana > max_mana :
		mana -= mana_decay_rate * delta
		if mana < max_mana :
			mana = max_mana
		excess_mana = mana - max_mana

func _update_resources():
	emit_signal("resources_changed", health, max_mana, mana, max_mana, excess_mana)

##############################################################
####State Logistics####

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
	return null

func _exit_state(old_state, new_state):
	match old_state :
		states.casting :
			if casting_spell :
				#only gets called if spell is still being cast
				if casting_spell.interuptable :
					casting_spell.interupt()
				elif not state in [states.disabled] :
					state = states.casting
					return
			casting_timer.stop()
			terminal_velocity = TERMINAL_VELOCITY
	pass

func _enter_state(new_state, old_state):
	match state:
		states.casting :
			casting_spell.start_casting()
			casting_timer.start(casting_spell.casting_time)
			terminal_velocity = Globals.CELL_SIZE * .1
			#add some sort of specific effects to the spell here
		states.idle :
			$IdleTimer.start()
#		states.idle:
#			sprite.play('idle')
#		states.run:
#			sprite.play('run')
#		states.jump:
#			sprite.play('jump')

func _end_recovery():
	_set_state(states.fall)

func _end_cast():
	mana -= casting_spell.casting_cost
	casting_spell.cast(self, staff.projectile_spawn_pos.global_position, Vector2.UP.rotated(staff.rotation))
	recovery_timer.start(casting_spell.recovery_time)
	cayote_timer.stop()
	casting_spell = null
	_set_state(states.recovering)

func _on_HitstunTimer_timeout():
	_set_state(states.idle)

func _save():
	var save_data = {
		"max_health" : max_health,
		"pos_x" : position.x,
		"pos_y" : position.y,
		"teleport" : $Teleport.known,
		"shield" : $Shield.known,
		"heal" : heal_known,
		"run" : run_known,
		"jumps" : jumps
		}
	for sp in spell_list.get_children() :
		var spell := sp as Spell
		save_data[spell.name] = {
			"known" : spell.known,
			"equipped" : spell.equipped
		}
	return save_data

func _load(dict := {}):
	max_health = dict.max_health
	position.x = dict.pos_x
	position.y = dict.pos_y
	heal_known = dict.heal
	run_known = dict.run
	jumps = dict.jumps
	yield(self, "ready")
	$Teleport.known = dict.teleport
	$Shield.known = dict.shield
	for spell in spell_list.get_children() :
		if not dict.has(spell.name) :
			continue
		else :
			var d = dict[spell.name]
			spell.known = d.known
			spell.equipped = d.equipped

func lerp_angle(from, to, weight):
    return from + short_angle_dist(from, to) * weight

func short_angle_dist(from, to):
    var max_angle = PI * 2
    var difference = fmod(to - from, max_angle)
    return fmod(2 * difference, max_angle) - difference