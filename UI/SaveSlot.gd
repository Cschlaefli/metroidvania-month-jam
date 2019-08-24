extends Control

export(PackedScene) var start_scene

export var file := "debug"
var info
var empty = true


func _ready():
	$Name.text = file
	update()

func update():
	info = Globals._load("user://" + file + ".json")
	empty = info.empty()
	$Delete.disabled = false
	if empty :
		$NewGame.visible = true
		$Delete.disabled = true

func _on_click():
	if empty :
		new_game()
	else :
		Globals.set_save(file)
		Globals.load_save()
	self.queue_free()

func new_game() :
	Globals.set_save(file)
	get_tree().change_scene_to(start_scene)

func delete():
	Globals.delete(file)
	update()
