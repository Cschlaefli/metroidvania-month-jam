extends Spell

export var reflect_bonus := .1
signal reflected(body)

func start_casting():
	casting = true
	$CastingEffect.emitting = true

func interupt():
	if interuptable :
		casting = false
		$CastingEffect.emitting = false

func cast(by : Node2D, point : Vector2 ,  direction : Vector2):
	rotation = direction.angle()
	$ActiveTimer.start(active_time)
	$Reflector.activate()
	casting = false
	$CastingEffect.emitting = false
	$Reflector/ShieldParticles.emitting = true

func _on_activeTimer_timeout():
	$Reflector.deactivate()
	$Reflector/ShieldParticles.emitting = false


func _on_Reflector_reflected(body):
	if not $ActiveTimer.is_stopped() :
		emit_signal("reflected", body)
		$ActiveTimer.start($ActiveTimer.time_left + reflect_bonus)
