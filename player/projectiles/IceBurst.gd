extends Area2D

func _ready():
	$CPUParticles2D.emitting = true

func _on_DespawnTimer_timeout():
	queue_free()


func _on_ActiveTimer_timeout():
	$CPUParticles2D.emitting = false
	$DespawnTimer.start()
