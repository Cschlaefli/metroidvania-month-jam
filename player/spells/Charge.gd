extends Spell

var dir : Vector2
var by : Enemy

func start_casting():
	casting = true
	by.velocity = -dir * 400

func interupt():
	casting = false
	$CastingEffect.emitting = false

func cast(by : Node2D, point : Vector2 ,  direction : Vector2):
	by.velocity = dir * 5000
	casting = false
	$CastingEffect.emitting = false
	pass
