extends Node2D

export var track_x := true
export var track_y := true

export var limit_top := 0
export var limit_bot := 4068
export var limit_left := 0
export var limit_right := 4068

func player_entered(camera, transition_position) :
	if Screens.current_screen == self :
		return
	Screens.current_screen = self
	var limit = $TileMap.get_used_rect().size * $TileMap.cell_size
	var limits = {
		'top' : position.y, 'bottom' : position.y + limit.y,
		'left' : position.x, 'right' : position.x + limit.x}

	camera.transition(transition_position, limits)
	yield(camera, "end_trans")


func _process(delta):
	pass