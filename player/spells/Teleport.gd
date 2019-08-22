extends Spell

var distance := Globals.CELL_SIZE * 4
var direction : int

onready var left_check := $Left
onready var right_check := $Right

func _ready():
	left_check.cast_to = Vector2.LEFT* distance
	right_check.cast_to = Vector2.RIGHT* distance

func cast(by : Node2D, point : Vector2 ,  d : Vector2):
	if not by is Player :
		return

	var dir := direction
	var dist := distance
	var check : RayCast2D
	if dir > 0 :
		check = right_check
	else :
		check = left_check

	if check.is_colliding() :
		dist = check.get_collision_point().distance_to(check.global_position)
#		dist -= dir * 32
		print(dist)

	by.position.x += dir * dist


