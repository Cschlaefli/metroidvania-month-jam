extends Area2D

class_name Pickup

var active := true

func add_ability(body : Player):
	pass

func _on_AbilityPickup_body_entered(body):
	if active and body is Player :
		add_ability(body as Player)
		active = false
		$Popup.show()
		yield(get_tree().create_timer(3.0), "timeout")
		$Popup.hide()
		$Particles2D.emitting = false

