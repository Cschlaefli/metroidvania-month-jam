extends Enemy

func _ready():
	._ready()
	$EnemyBody/Reflector.activate()

func _respawn():
	._respawn()
	$EnemyBody/Reflector.activate()
