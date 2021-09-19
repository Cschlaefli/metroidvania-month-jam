extends Area2D

export var reflect_hitmask := 9
export var reflect_mask := 4
var direction := Vector2.ZERO
export var speed_mod := 1.5
export var damage_mod := 1.2
export var direct_reflect := false

signal reflected(body)

func _ready():
	connect("area_entered", self, "_on_Reflector_entered")
	connect("body_entered", self, "_on_Reflector_entered")

func activate():
	collision_mask = reflect_mask

func deactivate():
	collision_mask = 0

func _on_Reflector_entered(body : Projectile):
#	if body.has_method("reflect") and body.reflectable :
	if body != null and body.reflectable :
		body.speed *= speed_mod
		body.damage *= damage_mod
		emit_signal("reflected", body)
		if direct_reflect :
			body.reflect(reflect_hitmask, body.direction.rotated(PI/2))
		else :
			body.reflect(reflect_hitmask, Vector2.UP.rotated(global_rotation))
