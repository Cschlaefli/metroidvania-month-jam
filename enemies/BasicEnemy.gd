extends Enemy

onready var moving_timer := $MovingTimer
onready var standing_timer := $StandingTimer
export var speed := 2.2


func _handle_agro(delta : float):
	velocity.x = lerp(velocity.x, 0, delta * 5)
	if shoot_timer.is_stopped() :
		cast()
		shoot_timer.start(shoot_interval)

func _handle_idle(delta : float):
	_handle_agro(delta)

func _handle_casting(delta) :
	pass

func _handle_recovery(delta) :
#	velocity.x = lerp(velocity.x, 0, delta)
#	velocity.y += gravity * .5
	pass

func cast() :
	casting_spell = $EnemyBody/Hop
	casting_spell.dir = player_dir
	casting_spell.by = self
	_set_state(states.casting)

func _on_Hurtbox_hit(body):
	velocity.x = - velocity.x
