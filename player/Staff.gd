extends Node2D

func _physics_process(delta):
	
	rotation = (get_global_mouse_position() - global_position).angle() + PI / 2
	
	if Input.is_action_just_pressed('shoot'):
		_shoot()

func _shoot():
	print('shot')