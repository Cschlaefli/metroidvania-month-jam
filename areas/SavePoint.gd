extends Entrance
tool
var active := false

func _process(delta):
	var bods = get_overlapping_bodies()
	active = false
	for bod in bods :
		if bod is Player :
			active = true

func _input(event):
	if active  and event.is_action_pressed("ui_accept") :
		Globals.save()

