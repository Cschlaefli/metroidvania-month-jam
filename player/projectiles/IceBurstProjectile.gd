extends Projectile

func _on_Projectile_body_entered(body):
	
	if body.has_method("hit"):
		body.hit(self, damage, effect)

