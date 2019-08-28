extends Spell

var by : Enemy
var dir : Vector2
export var speed := 1500
export var jump_height := 2.0

func start_casting():
	by.velocity.x = -sign(dir.x) * speed * .25
	casting = true

func interupt():
	by.velocity.x = -by.velocity.x
	casting = false

func cast(by : Node2D, point : Vector2 ,  direction : Vector2):
	by.velocity.x = sign(dir.x) * speed
	by.velocity.y = -sqrt(2* by.gravity *jump_height *  Globals.CELL_SIZE * Globals.CELL_SIZE)

