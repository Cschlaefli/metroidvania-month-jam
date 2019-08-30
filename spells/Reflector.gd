extends Area2D

export var reflect_hitmask := 9
export var reflect_mask := 4
var direction := Vector2.ZERO

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
		emit_signal("reflected")
		body.reflect(reflect_hitmask, direction)

