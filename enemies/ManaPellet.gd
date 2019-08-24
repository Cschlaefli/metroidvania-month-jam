extends KinematicBody2D

var velocity := Vector2.ZERO
var speed = Globals.CELL_SIZE * 10
var amount := 5.0

onready var detect_area := $DetectionArea
onready var pellet := $Pellet
onready var dissolve_timer := $DissolveTimer
onready var vfx := $CPUParticles2D


func _physics_process(delta):
	if detect_area.get_overlapping_bodies().size() != 0:
		var target_velocity = (Globals.player.global_position - global_position).normalized() * speed
		velocity = lerp(velocity, target_velocity, delta * 5)
	else:
		velocity = lerp(velocity, Vector2.ZERO, delta)


	move_and_slide(velocity)

func _on_Pellet_body_entered(body):
	var player = body as Player

	if player:
		player.mana += amount
		amount = 0
		_dissolve()


func _dissolve():
	dissolve_timer.start()
	vfx.lifetime = lerp(vfx.lifetime, 0, .6)
	vfx.emitting = false


func _on_DissolveTimer_timeout():
	queue_free()
