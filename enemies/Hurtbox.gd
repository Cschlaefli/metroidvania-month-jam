extends Area2D

class_name Hurtbox

export var damage := 1.0
export var type := 0
export var knockback := Vector2.ZERO
export var hitstun_time := .1

signal hit

func _ready():
	connect("body_entered", self, "_on_hit")
	connect("area_entered", self, "_on_hit")

func _on_hit(body):
	if body.has_method("hit"):
		body.hit(self, damage, type, knockback * Globals.CELL_SIZE, hitstun_time)
		emit_signal("hit", body)
