extends Node2D

var spawn_points = []
var debug := true
export var spawn_at = 0


func _ready():
	for point in $Spawns.get_children() :
		spawn_points.append(point.global_position)

	if debug :
		var p = Globals.PLAYER.instance()
		p.position = spawn_points[spawn_at]
		add_child(p)

	Globals.current_area = self


func _save(file : String):
	pass

func _load(file : String):
	pass
