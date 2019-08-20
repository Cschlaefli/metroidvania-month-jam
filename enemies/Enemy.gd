extends Node2D
class_name Enemy

var velocity = Vector2.ZERO

export var max_hp := 1.0
export var damage := 1.0
export var gravity = 80
export var mana_dropped := 1
onready var mana := preload('res://enemies/ManaPellet.tscn')
var hp := 1.0

var dead := false
var ENEMY : PackedScene
onready var curr_enemy := $EnemyBody

signal die

func _ready() :
	ENEMY = PackedScene.new()
	for child in curr_enemy.get_children() : child.owner = curr_enemy
	ENEMY.pack(curr_enemy)
	hp = max_hp

func _physics_process(delta):
	if not dead :
		if not curr_enemy.is_on_floor():
			velocity.y += gravity

		curr_enemy.move_and_slide(velocity, Vector2.DOWN)


func hit(by : Node2D, damage : float, type : int, knockback := Vector2.ZERO):
	hp -= damage
	if hp <= 0:
		on_death()

func on_death():
	for i in range(0,mana_dropped):
		var to_add = mana.instance()
		to_add.position = curr_enemy.position
		to_add.velocity = Vector2.UP * Globals.CELL_SIZE * 2
		to_add.rotation = rand_range(0,2 * PI)
		add_child(to_add)
	curr_enemy.queue_free()
	dead = true
	curr_enemy = null

func respawn():
	velocity = Vector2.ZERO
	if dead :
		curr_enemy = ENEMY.instance()
		add_child(curr_enemy)
		curr_enemy.connect("hit", self, "hit")

	hp = max_hp
	curr_enemy.position = Vector2.ZERO

	dead = false

func sleep():
	pass
func wake():
	pass

func _die():
	emit_signal("die")
	for i in range(0,mana_dropped):
		var to_add = mana.instance()
		to_add.global_position = global_position
		to_add.velocity = Vector2.UP * Globals.CELL_SIZE * 2
		to_add.rotation = rand_range(0,2 * PI)
		get_tree().get_root().add_child(to_add)
	queue_free()
