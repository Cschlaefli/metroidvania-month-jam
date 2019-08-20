extends Enemy

onready var moving_timer := $MovingTimer
onready var standing_timer := $StandingTimer
onready var facing_cast := $FacingCast

func _ready():
	moving_timer.start()
#	velocity.x = (Globals.player.global_position - global_position).normalized().x * Globals.CELL_SIZE * 1.5

func _physics_process(delta):
	._physics_process(delta)

func _on_MovingTimer_timeout():
	standing_timer.start()
	velocity.x = 0


func _on_StandingTimer_timeout():
	moving_timer.start()
	velocity.x = (Globals.player.global_position - global_position).normalized().x * Globals.CELL_SIZE * 1.5