extends Spell

var dir : Vector2
var by : Enemy
export var distance := 4.0
export var draw_back := .5
func start_casting():
	casting = true
	by.velocity = -dir * (draw_back * Globals.CELL_SIZE)/casting_time

func interupt():
	casting = false
	$CastingEffect.emitting = false

func cast(by : Node2D, point : Vector2 ,  direction : Vector2):
	by.velocity = dir * (distance * Globals.CELL_SIZE)/casting_time
	casting = false
	$CastingEffect.emitting = false
	pass
