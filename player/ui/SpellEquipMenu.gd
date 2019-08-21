extends Control

var spells
var displayed_spells = []
var spell_toggle = preload("res://player/ui/SpellToggleButton.tscn")

func _input(event):
	if event.is_action_pressed("ui_cancel") :
		toggle()

func update_display():
	displayed_spells = []
	for spell in spells :
		if spell.known :
			displayed_spells.append(spell)
	_display()

func _display():
	for spell in displayed_spells :
		var to_add = spell_toggle.instance()
		to_add.spell = spell
		$Spells.add_child(to_add)

func toggle():
	visible = !visible
	if visible :
		get_tree().paused = true
	else:
		get_tree().paused = false
