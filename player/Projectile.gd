extends Node2D

var direction := Vector2.ZERO
var speed := 1
var cd := 0.8
var cast_time := 0.8

onready var particles := $CPUParticles2D
onready var dissolve_timer := $DissolveTimer
onready var area := $Area2D

func _physics_process(delta):
	position += direction * speed

func _on_Area2D_body_entered(body):
	var terrain = body as TileMap
	
	if terrain:
		_dissolve()

func _dissolve():
	if dissolve_timer.is_stopped():
		area.monitoring = false
		speed = 12
		particles.emitting = false
		dissolve_timer.start()

func _on_DissolveTimer_timeout():
	queue_free()
