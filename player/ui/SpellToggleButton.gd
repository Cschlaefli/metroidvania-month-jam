extends TextureButton

var spell : Spell

func _ready():
	texture_normal = spell.menu_tex
	$Info/SpellCost.text = "Cost : %s" % spell.casting_cost
	$Info/SpellName.text =  spell.spell_name
	if spell.equipped :
		self_modulate.a = .8
	else :
		self_modulate.a = .4

func _pressed():
	spell.equipped = !spell.equipped
	spell.emit_signal("updated")
	if spell.equipped :
		self_modulate.a = .8
	else :
		self_modulate.a = .4