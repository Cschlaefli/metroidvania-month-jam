extends Node2D

export var active := false
export var shot_spacing := .5
export var barrage_delay := 4
export var random_order := true
var time = 0
var spells = []

func _ready():
	_set_spells()

func _set_spells():
	for child in get_children() : if child is Spell : spells.append(child)
	if random_order : spells.shuffle()

func _fire_next():
	var spell : Spell = spells.pop_front()
	if not spell :
		_set_spells()
		return barrage_delay
	else :
		var player_dir = ( Globals.player.global_position - spell.global_position)
		spell.cast(self, spell.global_position, player_dir)
		return shot_spacing

func _physics_process(delta):
	time -= delta
	if active and time <= 0 :
		time = _fire_next()