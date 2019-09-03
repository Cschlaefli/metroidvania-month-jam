extends Node

const CELL_SIZE := 256

var current_screen : Screen
var current_area : Node2D
var player : Node2D

var current_save := "user://debug.json"

var PLAYER = load("res://player/Player.tscn")

var mouse_aim := true

signal new_screen
signal new_area

var save_buffer := {}
var load_buffer := {}

func _ready():
	pass

func player_death():
	save_buffer.clear()
	load_save()

func set_save(file : String):
	current_save = "user://" + file + ".json"

func save():
	save_buffer[current_area.name] = current_area._save()
	save_buffer["player"] = player._save()
	save_buffer["current_area"] = current_area.filename
	_save()

func load_save(file := current_save) :
	load_buffer = _load(file)
	if load_buffer.size() == 0 :
		return

	current_screen = null
	player = PLAYER.instance()

	if current_area :
		current_area.queue_free()
		yield(current_area, "tree_exited")

	current_area = load(load_buffer.current_area).instance()
	current_area.debug = false
	get_tree().root.add_child(current_area)
	current_area.add_child(player)
	current_area._load(load_buffer[current_area.name])
	player._load(load_buffer["player"])

func _save():
	var save := File.new()
	load_buffer = _load(current_save)
	for key in save_buffer.keys() :
		load_buffer[key] = save_buffer[key]
	save.open(current_save, File.WRITE)
	save.store_string(to_json(load_buffer))
	save.close()
	save_buffer.clear()

func _load(file):
	var save := File.new()
	if not save.file_exists(file) :
		return {}

	save.open(file, File.READ)
	var lb = parse_json(save.get_as_text())
	save.close()
	return lb

func delete(file):
	var tmp = "user://" + file + ".json"
	save_buffer = {}
	load_buffer = {}
	var save := File.new()
	save.open(tmp, File.WRITE)
	save.store_string(to_json(load_buffer))
	save.close()

func change_area(new_area, position):
	#some transition screenfade here
	save_buffer[current_screen.name] = current_area._save()
	current_area.remove_child(player)
	current_area.queue_free()
	yield(current_area, "tree_exited")
	current_area = new_area
	new_area.debug = false
	get_tree().root.add_child(current_area)
	if save_buffer.has(new_area.name):
		new_area._load(save_buffer[new_area.name])
	elif load_buffer.has(new_area.name):
		new_area._load(load_buffer[new_area.name])

	player.global_position = new_area.spawn_points[position]
	new_area.add_child(player)
