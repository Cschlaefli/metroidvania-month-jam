extends TextureButton

var spell : Spell

func _ready():
	texture_normal = spell.menu_tex
	if spell.equipped :
		modulate.a = .8
	else :
		modulate.a = .4

func _pressed():
	spell.equipped = !spell.equipped
	spell.emit_signal("updated")
	if spell.equipped :
		modulate.a = .8
	else :
		modulate.a = .4