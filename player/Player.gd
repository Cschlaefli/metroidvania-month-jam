extends KinematicBody2D

class_name Player

var velocity = Vector2.ZERO

var health := 10.0
var max_health := 10.0

var heal_rate := 2.0

var mana := 10.0
var max_mana := 10.0

var mana_regen_rate := 1.0
var mana_decay_rate := .25
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

onready var footstep_sfx := $Footstep
onready var jump_sfx := $Jump
onready var cast_sfx := $Cast

onready var spell_list = $SpellList
var equipped_spells =  []
var current_spell : Spell
var casting_spell : Spell

onready var spell_equip_menu = $CanvasLayer/SpellEquipMenu
onready var resource_display = $CanvasLayer/ResourceDisplay

signal spell_list_changed(equipped_spells)

signal resources_changed(health, max_health, mana, max_mana, excess_mana)

func _ready():
	Globals.player = self
	for spell in Spells.SPELL_LIST :
		var to_add = spell.instance() as Spell
		spell_list.add_child(to_add)
		to_add.connect("updated", self, "_update_spells")

	spell_equip_menu.spells = spell_list.get_children()
	spell_equip_menu.update_display()

	_update_spells()
	if equipped_spells.size() > 0 :
		current_spell = equipped_spells[0]
	$Teleport.sprite_mat = $AnimatedSprite.material
	_update_resources()
	_add_state('idle')
	_add_state('healing')
	_add_state('run')
	_add_state('jump')
	_add_state('fall')
	_add_state('casting')
	_add_state('recovering')
	_add_state('hitstun')
	_add_state('disabled')
	_add_state('charging')
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
		if event.is_action_released('shoot'):
			if current_spell :
				if current_spell.casting_cost <= mana :
					casting_spell = current_spell
					_set_state(states.casting)
		elif event.is_action_pressed("heal") and heal_known and excess_mana > 0 and health < max_health :
			_set_state(states.healing)
		elif event.is_action_pressed('teleport') and $Teleport.known:
			var spell = $Teleport
			if spell.casting_cost <= mana :
				casting_spell = spell
				casting_spell.direction = facing_direction
				_set_state(states.casting)

func _cycle_spells(forward := true) :
	if equipped_spells.size() <= 1: return
	if current_spell : current_spell.guide = false
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
	match state :
			states.recovering :
				_handle_movement(delta * .05)
				_handle_jumping()
			states.casting :
				_cast_arrest(delta)
			states.healing :
				_handle_healing(delta)
			states.hitstun :
				pass
			states.disabled :
				pass
			_:
				_handle_movement(delta)
				_handle_jumping()
				if current_spell != null and current_spell.charging :
					_regen_mana(delta * .5)
					_charging(delta)
				else :
					_regen_mana(delta)
	_handle_camera(delta)
	_apply_velocity()
	_update_resources()

var look_distance_y = Globals.CELL_SIZE * 3
var look_distance_x = Globals.CELL_SIZE * 3

func _handle_camera(delta):
	var mouse_pos =  get_local_mouse_position()
	#offset camera if moving a direction but aiming a direction takes precidence

	var x
	var y
	var x_dist = clamp(mouse_pos.x, -look_distance_x, look_distance_x)
	var y_dist = clamp(mouse_pos.y, -look_distance_y, look_distance_y)
	if Globals.mouse_aim :
		x = clamp(abs(mouse_pos.x), 1, 1000) * sign(mouse_pos.x)/1000
		y = clamp(abs(mouse_pos.y), 1, 1000) * sign(mouse_pos.y)/1000
	else :
		x = Input.get_action_strength("aim_right") - Input.get_action_strength("aim_left")
		y = Input.get_action_strength("aim_down") - Input.get_action_strength("aim_up")
		x_dist = look_distance_x*x
		y_dist = look_distance_y*y

	y = clamp(abs(y*10), .5, 2)
	cam.position.y = lerp(cam.position.y, y_dist, delta * abs(y))

	x = clamp(abs(x*10), 2, 4)
	cam.position.x = lerp(cam.position.x, x_dist, delta * abs(x))





func _handle_healing(delta):
	velocity.x = lerp(velocity.x, 0, delta * player_deceleration)
	if excess_mana > 0 and health < max_health and Input.is_action_pressed("heal") :
		var rate = heal_rate * delta
		mana -= rate
		excess_mana = mana - max_mana
		health += rate
	else :
		_set_state(states.idle)

func _charging(delta):
	velocity.x = lerp(velocity.x, 0, delta * 5)
	velocity.y = lerp(velocity.y, 0, delta * 10)

func _cast_arrest(delta):
	if not casting_spell.loose_casting :
		velocity.x = lerp(velocity.x, 0, delta * 5)

func _handle_gravity(delta):
	if is_on_ceiling():
		velocity.y += Globals.CELL_SIZE

	if is_on_floor():
		if not state == states.hitstun :
			velocity.y = lerp(velocity.y, 0 , delta * 3)
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

var cast_dir := Vector2.ZERO

func _handle_weapon(delta):
	if current_spell and Input.is_action_pressed("shoot") :
		current_spell.guide = true
		if current_spell.chargable :
			current_spell.charging = true
		current_spell.can_cast = current_spell.casting_cost <= mana
		resource_display.show_cost(current_spell.casting_cost, current_spell.can_cast)
	else :
		if current_spell and current_spell.chargable : 
			current_spell.guide = false
			current_spell.charging  = false
		resource_display.show_cost(0.0)

	if not Globals.mouse_aim :
		var temp = Vector2.ZERO
		temp.x = Input.get_action_strength("aim_right") - Input.get_action_strength("aim_left")
		temp.y = Input.get_action_strength("aim_down") - Input.get_action_strength("aim_up")
#		if temp == Vector2.ZERO : aim to move felt wrong
#			temp.x = Input.get_action_strength("move_right") - Input.get_action_strength("move_left")
#			temp.y = Input.get_action_strength("look_down") - Input.get_action_strength("look_up")
		if temp !=  Vector2.ZERO :
			cast_dir = temp.normalized()
	else :
		cast_dir = cast_dir.normalized()
		cast_dir = (get_global_mouse_position() - global_position)

	var rot = cast_dir.angle() + PI / 2
	if state == states.casting :
		staff.rotation = lerp_angle(staff.rotation, rot, delta * 3)
	else :
		staff.rotation = lerp_angle(staff.rotation, rot, delta * 10 )
	cast_dir = Vector2(cos(staff.rotation), sin(staff.rotation)).rotated(-PI/2)

func _handle_jumping():
	if Input.is_action_just_pressed('jump') :
		if is_on_floor() or not cayote_timer.is_stopped() :
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
				#disabled overrides casting, but otherwise stuck in casting
				if casting_spell.interuptable :
					casting_spell.interupt()
				elif not state in [states.disabled] :
					state = states.casting
					return
			casting_timer.stop()
			terminal_velocity = TERMINAL_VELOCITY
		states.healing :
			$HealEffect.emitting = false
		states.run :
			pass
#			footstep_sfx.stop()
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
		states.healing :
			$HealEffect.emitting = true
#		states.idle:
#			sprite.play('idle')
		states.run:
			pass
#			footstep_sfx.play()
#			sprite.play('run')
		states.jump:
			jump_sfx.play()
#			sprite.play('jump')

func _end_recovery():
	_set_state(states.fall)

func _end_cast():
	cast_sfx.play()
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
		"default_jumps" : default_jumps
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
	default_jumps = dict.default_jumps
	$Teleport.known = dict.teleport
	$Shield.known = dict.shield
	for spell in spell_list.get_children() :
		if not dict.has(spell.name) :
			continue
		else :
			var d = dict[spell.name]
			spell.known = d.known
			spell.equipped = d.equipped
	_update_spells()

func lerp_angle(from, to, weight):
	return from + short_angle_dist(from, to) * weight

func short_angle_dist(from, to):
	var max_angle = PI * 2
	var difference = fmod(to - from, max_angle)
	return fmod(2 * difference, max_angle) - difference

var reflect_bonus = 1.5

func _on_Shield_reflected(body):
	mana += reflect_bonus
