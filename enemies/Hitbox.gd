extends KinematicBody2D

signal hit(by, damage, type, knockback)

func hit(by : Node2D, damage : float, type : int, knockback := Vector2.ZERO):
	emit_signal("hit", by, damage, type, knockback)