extends Area2D

export var reflect_hitmask := 9
export var reflect_mask := 4
var direction := Vector2.ZERO
export var speed_mod := 1.5
export var damage_mod := 1.2

signal reflected

func activate():
	collision_mask = reflect_mask
	$ShieldParticles.emitting = true
#	$ShieldParticles.restart()

func deactivate():
	collision_mask = 0
	$ShieldParticles.emitting = false

func _on_Reflector_entered(body):
	if body.has_method("reflect") and body.reflectable :
		body.speed *= speed_mod
		emit_signal("reflected")
		body.reflect(reflect_hitmask, direction)

