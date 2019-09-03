extends TextureButton

var spell : Spell

func _ready():
	texture_normal = spell.menu_tex
	$Info/SpellCost.text = "Cost : %s" % spell.casting_cost
	$Info/SpellName.text =  spell.spell_name
	self_modulate.a = .4
	_update()

func _pressed():
	spell.equipped = !spell.equipped
	spell.emit_signal("updated")
	_update()

func _update():
	if spell.equipped :
		self_modulate.r = .8
		self_modulate.b = .8
		self_modulate.g = .8
	else :
		self_modulate.g = .2
		self_modulate.b = .2
		self_modulate.r = .2

func _on_SpellToggleButton_focus_entered():
	self_modulate.a = 1.0

func _on_SpellToggleButton_focus_exited():
	self_modulate.a = .6


