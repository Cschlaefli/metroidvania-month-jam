extends KinematicBody2D
class_name Enemy

var velocity = Vector2.ZERO

export var max_hp := 1.0
export var damage := 1.0
export var gravity = 80
var hp := 1.0


signal die

func _ready() :
	hp = max_hp

func _physics_process(delta):
	if !is_on_floor():
		velocity.y += gravity

	move_and_slide(velocity, Vector2.DOWN)


func hit(by : Node2D, damage : float, type : int, knockback := Vector2.ZERO):
	hp -= damage
	if hp <= 0:
		_die()


func _die():
	emit_signal("die")
	queue_free()