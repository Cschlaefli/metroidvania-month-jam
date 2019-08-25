extends Enemy

onready var moving_timer := $MovingTimer
onready var standing_timer := $StandingTimer
onready var facing_cast := $FacingCast
export var speed := 2.2

func _ready():
	moving_timer.start()
#	velocity.x = (Globals.player.global_position - global_position).normalized().x * Globals.CELL_SIZE * 1.5


func _handle_agro(delta : float):
	pass

func _handle_idle(delta : float):
	pass

func _on_MovingTimer_timeout():
	standing_timer.start()
	velocity.x = 0

func _on_StandingTimer_timeout():
	moving_timer.start()
	velocity.x = (Globals.player.global_position - global_position).normalized().x * Globals.CELL_SIZE * speed
