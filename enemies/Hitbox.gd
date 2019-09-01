extends CollisionObject2D

onready var controller := get_parent()

signal hit(by, damage, type, knockback, hitstun_time)

func hit(by : Node2D, damage : float, type : int, knockback := Vector2.ZERO, hitstun_time := .1):
	emit_signal("hit", by, damage, type, knockback, hitstun_time)


