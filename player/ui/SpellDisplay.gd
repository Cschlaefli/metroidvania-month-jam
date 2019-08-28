extends Node2D

var spell_list = []
var offset = 64
onready var tween = Tween.new()
func _ready():
	add_child(tween)

func update_list(sp):
	for child in get_children() : child.queue_free()
	spell_list = sp.duplicate()
	sp = spell_list

	if sp.size() == 0 :
		return
	if sp.size() >=3 :
		sp.push_front(sp.pop_back())
	elif sp.size() == 2 :
		sp.append(sp[0])
		sp.push_front(sp[1])
	else :
		sp.append(sp[0])
		sp.append(sp[0])


	for x in range(0, spell_list.size()) :
		var to_add := Sprite.new()
		var spell = spell_list[x]
		to_add.texture = spell.menu_tex
		to_add.position.y = offset * x
		if x != 1 :
			to_add.scale *= .75
			to_add.modulate.a = .5
		if x >= 3 :
			to_add.scale *= .75
			to_add.modulate.a = .0
		add_child(to_add)

func cycle(forward := true):
	pass
#	var top = spell_list[0]
#	var curr = spell_list[1]
#	var bot = spell_list[2]
#
#	var new
#	var next
#	if forward :
#		next = bot
#		new = spell_list[4]
#	else :
#		next = top
#		new = spell_list[-1]

