extends Projectile



func _on_Area2D_body_entered(body):
	._on_Area2D_body_entered(body)
	var enemy = body as Enemy

	if enemy:
		enemy._set_state('fear')
