extends Node2D
class_name Projectile

export var speed := 5.0
export var cd := 0.5
export var cast_time := 0.25
export var damage := 1.0
var direction := Vector2.ZERO

onready var particles := $CPUParticles2D
onready var dissolve_timer := $DissolveTimer
onready var area := $Area2D

func _physics_process(delta):
	position += direction * speed

func _on_Area2D_body_entered(body):
	var terrain = body as TileMap
	var enemy = body as Enemy

	if terrain:
		_dissolve()
	elif enemy:
		enemy.hit(self, damage, 0)
		_dissolve()

func _dissolve():
	if dissolve_timer.is_stopped():
		area.monitoring = false
		speed = 12.5
		particles.emitting = false
		dissolve_timer.start()

func _on_DissolveTimer_timeout():
	queue_free()
