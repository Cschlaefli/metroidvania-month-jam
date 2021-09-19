extends Area2D
class_name Projectile

export var speed := 5.0
export var damage := 1.0
export var knockback := Vector2.ZERO
export var hitstun := .2
export var reflectable := true
export var max_reflects := 1 << 12
export var explodes := false
var reflect_count := 0
export var dissolves := true
export(Damage.dam_types) var effect := 0 
var direction := Vector2.ZERO

onready var particles := $CPUParticles2D
onready var dissolve_timer := $DissolveTimer

func reflect(new_hitmask, new_direction := -direction):
	reflect_count += 1
	if reflect_count >= max_reflects :
		 _explode()
	else : 
		collision_mask = new_hitmask
		direction = direction.reflect(new_direction)
		rotation = direction.angle()

func _physics_process(delta):
	if not direction.is_normalized() : direction.normalized()
	position += direction * speed

func _on_Projectile_body_entered(body):
	var terrain = body as TileMap

	if terrain:
		if dissolves : _dissolve()
	elif body.has_method("hit"):
		body.hit(self, damage, effect, knockback, hitstun)
		if dissolves : _dissolve()

func _explode():
	pass

func _dissolve():
	if dissolve_timer.is_stopped():
		collision_mask = 0
		speed = 12.5
		particles.emitting = false
		dissolve_timer.start()

func _on_DissolveTimer_timeout():
	queue_free()
