extends Control

var spells
var displayed_spells = []
var spell_toggle = preload("res://player/ui/SpellToggleButton.tscn")

func _input(event):
	if event.is_action_pressed("pause") :
		toggle()

func update_display():
	displayed_spells = []
	for spell in spells :
		if spell.known :
			displayed_spells.append(spell)
	_display()

func _display():
	var sps = []
	for spell in displayed_spells :
		var to_add = spell_toggle.instance()
		to_add.spell = spell
		$Spells.add_child(to_add)
		sps.append(to_add)
	if sps.size() == 0 :return
#	sps[0].grab_focus()
#	for x in range(0, sps.size()-2) :
#		var curr := sps[x] as BaseButton
#		var next := sps[x+1] as BaseButton
#		curr.focus_neighbour_right = next.get_path()
#		next.focus_neighbour_left = curr.get_path()
	sps.back().focus_neighbour_right = sps.front().get_path()
	sps.front().focus_neighbour_left = sps.back().get_path()


func toggle():
	visible = !visible
	if visible :
		$Spells.get_children().front().grab_focus()
		get_tree().paused = true
	else:
		get_tree().paused = false
