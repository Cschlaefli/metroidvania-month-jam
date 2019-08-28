extends Area2D

func _ready():
	$CPUParticles2D.emitting = true
	monitoring = true

func _on_DespawnTimer_timeout():
	queue_free()


func _on_ActiveTimer_timeout():
	$CPUParticles2D.emitting = false
	collision_mask = 0
	$DespawnTimer.start()


func _on_IceBurst_body_entered(body):
	var enemy = body as Enemy
	
	if enemy:
		print('slowed an enemy')
