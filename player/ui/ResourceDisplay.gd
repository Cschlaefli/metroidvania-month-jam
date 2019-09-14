extends Control

onready var health_bar = $HealthBar
onready var mana_bar = $ManaBar
onready var excess_bar = $ExcessBar
onready var cost_mat : ShaderMaterial = $CostBar.material

var _health
var _max_health
var _mana
var _max_mana
var _mana_threshold
var _excess_mana := 0.0

func show_cost(cost, can_cast = true):
	cost_mat.set_shader_param("can_cast", can_cast)
	cost_mat.set_shader_param("cost", cost)

	if _excess_mana < cost :
		cost_mat.set_shader_param("max_mana", _max_mana)
		cost_mat.set_shader_param("curr", mana_bar.value + excess_bar.value)
	else :
		cost_mat.set_shader_param("max_mana", _max_mana  * 2)
		cost_mat.set_shader_param("curr", excess_bar.value)

func _on_resource_change(health, max_health, mana, max_mana, excess_mana):
	_health = health
	_max_health = max_health
	_max_mana = max_mana
	_mana = mana
	_excess_mana = excess_mana
	if health_bar.max_value != _max_health : health_bar.max_value = _max_health
	if mana_bar.max_value != _max_mana : mana_bar.max_value = _max_mana
	if excess_bar.max_value != _max_mana * 2 : excess_bar.max_value = _max_mana

func _process(delta):

	excess_bar.value = lerp(excess_bar.value, _excess_mana, delta * 5)
	health_bar.value = lerp(health_bar.value, _health, delta * 5)
	mana_bar.value = lerp(mana_bar.value, _mana, delta * 5)

