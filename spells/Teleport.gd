extends Spell

var distance = Globals.CELL_SIZE * 4
var direction : int

onready var left_check := $Left
onready var right_check := $Right
onready var left_bod := $LeftBod
onready var right_bod := $RightBod

var sprite_mat : ShaderMaterial

var time = 0

func _physics_process(delta):
	if casting :
		time += 1
		sprite_mat.set_shader_param("amount", sin(time *5) *30)

func start_casting():
	casting = true

func interupt():
	if interuptable :
		casting = false
		sprite_mat.set_shader_param("amount", 0)

func _ready():
	left_check.cast_to = Vector2.LEFT* distance
	right_check.cast_to = Vector2.RIGHT* distance
	left_bod.position = left_check.cast_to
	right_bod.position = right_check.cast_to


func cast(by : Node2D, point : Vector2 ,  d : Vector2):
	if not by is Player :
		return

	sprite_mat.set_shader_param("amount", 0)

	var dir := direction
	var dist = distance
	var check : RayCast2D
	var body : Area2D
	if dir > 0 :
		check = right_check
		body = right_bod
	else :
		check = left_check
		body = left_bod

	if check.is_colliding() :
		dist = check.get_collision_point().distance_to(check.global_position)
		if body.get_overlapping_bodies().empty() and body.get_overlapping_areas().empty() :
			var cam = Globals.player.cam
			if body.global_position.x < cam.limit_right and body.global_position.x > cam.limit_left and body.global_position.y > cam.limit_top and body.global_position.y < cam.limit_bottom :
				dist = distance

	by.velocity.y = 0
	by.position.x += dir * dist
	.cast(by, point, d)


