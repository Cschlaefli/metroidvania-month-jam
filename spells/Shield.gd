extends Spell

export var reflect_bonus := .2

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
	$Reflector.direction = Vector2.UP.rotated(rotation)
	casting = false
	$CastingEffect.emitting = false

func _on_activeTimer_timeout():
	$Reflector.deactivate()


func _on_Reflector_reflected():
	if not $ActiveTimer.is_stopped() :
		$ActiveTimer.start($ActiveTimer.time_left + reflect_bonus)
