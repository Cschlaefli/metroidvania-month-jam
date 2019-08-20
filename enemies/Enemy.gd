extends KinematicBody2D
class_name Enemy

var velocity = Vector2.ZERO

export var max_hp := 1.0
export var damage := 1.0
export var gravity = 80
export var mana_dropped := 1
onready var mana := preload('res://enemies/ManaPellet.tscn')
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
	for i in range(0,mana_dropped):
		var to_add = mana.instance()
		to_add.global_position = global_position
		to_add.velocity = Vector2.UP * Globals.CELL_SIZE * 2
		to_add.rotation = rand_range(0,2 * PI)
		get_tree().get_root().add_child(to_add)
	queue_free()
