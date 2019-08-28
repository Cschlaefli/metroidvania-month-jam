extends Area2D
class_name Projectile

export var speed := 5.0
export var cd := 0.5
export var cast_time := 0.25
export var damage := 1.0
export(Damage.dam_types) var effect := 0 
var direction := Vector2.ZERO

onready var particles := $CPUParticles2D
onready var dissolve_timer := $DissolveTimer

func _physics_process(delta):
	position += direction * speed

func _on_Projectile_body_entered(body):
	var terrain = body as TileMap

	if terrain:
		_dissolve()
	elif body.has_method("hit"):
		body.hit(self, damage, effect)
		_dissolve()

func _dissolve():
	if dissolve_timer.is_stopped():
		collision_mask = 0
		speed = 12.5
		particles.emitting = false
		dissolve_timer.start()

func _on_DissolveTimer_timeout():
	queue_free()
