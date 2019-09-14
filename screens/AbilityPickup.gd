extends Area2D

class_name Pickup

onready var label = $CanvasLayer/Popup/Label
var active := true

func add_ability(body : Player):
	pass

func _on_AbilityPickup_body_entered(body):
	if active and body is Player :
		add_ability(body as Player)
		active = false
		body._update_spells()
		$Particles2D.emitting = false
		$CanvasLayer/Popup.show()
		yield(get_tree().create_timer(3.0), "timeout")
		$CanvasLayer/Popup.hide()

