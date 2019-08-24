extends Node

const CELL_SIZE := 256

var current_screen : Screen
var current_area : Node2D
var player : Player

var current_save := "debug.txt"

const PLAYER = preload("res://player/Player.tscn")

signal new_screen
signal new_area

func _ready():
	pass

func change_area(new_area, position):
	#some transition screenfade here
	current_area._save(current_save)
	player._save(current_save)
	current_area.remove_child(player)
	current_area.queue_free()
	yield(current_area, "tree_exited")
	current_area = new_area
	new_area.debug = false
	get_tree().root.add_child(current_area)
	new_area._load(current_save)
	player.global_position = new_area.spawn_points[position]
	print(player.position)
	new_area.add_child(player)
	print(player.position)

