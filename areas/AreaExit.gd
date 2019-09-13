extends Area2D

class_name AreaExit

export(String, FILE) var NextArea
var next
export var player_pos := 0
var active := false

signal area_change(new_area)

func _ready():
	connect("body_entered", self, "_on_body_entered")
	connect("body_exited", self, "_on_body_exited")
	next = load(NextArea)

func _on_body_exited(body) :
	if body is Player :
		active = false

func _on_body_entered(body) :
	if body is Player :
		active = true

func _input(event):
	if active and event.is_action_pressed("ui_accept") :
		if not next : return
		var n = next.instance()
		Globals.change_area(n, player_pos)


