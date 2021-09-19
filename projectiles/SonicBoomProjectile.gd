extends Projectile

var boom = preload('res://projectiles/SonicBoomBurst.tscn')

func _dissolve():
	_explode()
	._dissolve()

func _explode():
	._dissolve()
	var add = boom.instance()
	add.global_position = global_position
	get_parent().add_child(add)
