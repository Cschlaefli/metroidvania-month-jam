extends Control

onready var health_bar = $HealthBar
onready var mana_bar = $ManaBar
onready var threshold_bar = $ThresholdBar

var _health
var _max_health
var _mana
var _max_mana
var _mana_threshold

func _on_resource_change(health, max_health, mana, max_mana, mana_threshold):
	_health = health
	_max_health = max_health
	_max_mana = max_mana
	_mana = mana
	_mana_threshold = mana_threshold

func _process(delta):
	if health_bar.max_value != _max_health : health_bar.max_value = _max_health
	if mana_bar.max_value != _max_mana : mana_bar.max_value = _max_mana
	if threshold_bar.max_value != _max_mana : threshold_bar.max_value = _max_mana

	#some threshold representation for the mana_threshold,
	#maybe a low alpha progress bar on top?

	threshold_bar.value = lerp(threshold_bar.value, _mana_threshold, delta * 5)
	health_bar.value = lerp(health_bar.value, _health, delta * 5)
	mana_bar.value = lerp(mana_bar.value, _mana, delta * 5)

