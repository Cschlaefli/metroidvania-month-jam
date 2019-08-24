extends Area2D

class_name AreaExit

export(PackedScene) var NextArea
export var player_pos := 0

signal area_change(new_area)

func _on_body_entered(body) :
	if body is Player :
		var next = NextArea.instance()
		Globals.change_area(next, player_pos)



