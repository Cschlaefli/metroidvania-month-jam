extends Node2D

class_name Spell

export var casting_time := .1
export var casting_cost := 1.0
export var recovery_time := .1
export var known := false
export var equipped := false
export var spell_name := "placeholder"
export var active_time := .1
export var hitstun := .3
export var projectile_damage := 1.0
export var projectile_speed := 30.0
export var recoil := 1000.0
export var loose_casting := false
export var recoil_dir := Vector2.ZERO
var guide := false
var can_cast := true
var current := false
var casting := false

export(int) var hitmask = 9

signal updated

export var menu_tex : Texture
export(NodePath) var projectile_path = "Projectiles"
var projectiles : Node
export var interuptable := true
export var chargable := false
export var max_charge := 10.0
var charging := false
var charge_value := 0.0

func _ready():
	projectiles = get_node(projectile_path)

func start_casting():
	casting = true
	$CastingEffect.emitting = true

func interupt():
	if interuptable :
		casting = false
		$CastingEffect.emitting = false

func cast(by : Node2D, point : Vector2 ,  direction : Vector2):
	casting = false
	$CastingEffect.emitting = false
	$ActiveTimer.start(active_time)

func _on_activeTimer_timeout():
	pass

func _show_guide(delta) :
	pass

func _physics_process(delta):
	if chargable and charging :
		charge_value += delta
		charge_value = min(charge_value, max_charge)
	else :
		charge_value = 1
	_show_guide(delta)
	update()

