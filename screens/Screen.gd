extends Node2D
class_name Screen

signal player_exited
signal player_entered

func player_entered(camera, transition_position) :
	if Globals.current_screen == self :
		return
	elif is_instance_valid(Globals.current_screen) :
		Globals.current_screen.player_exited()
	emit_signal("player_entered")
	Globals.emit_signal("new_screen", self)
	Globals.current_screen = self
	var limit = $TileMap.get_used_rect().size * $TileMap.cell_size
	var limits = {
		'top' : position.y , 'bottom' : position.y + limit.y,
		'left' : position.x , 'right' : position.x + limit.x}

	camera.transition(transition_position, limits)
	yield(camera, "end_trans")

func player_exited():
	emit_signal("player_exited")

func _process(delta):
	pass