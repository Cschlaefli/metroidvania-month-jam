extends Node2D

export var respawn_screens := 5
var screen_changes = respawn_screens

func _ready():
	Globals.connect("new_screen", self, "new_screen")

func player_entered():
	#activate enemies
	screen_changes = respawn_screens

func player_exited():
	#deactivate enemies
	pass

func new_screen(screen):
	#check if currenct screen
	screen_changes -=1
	if screen_changes <= 0 :
		reset()

func reset():
	for child in get_children() :
		if child is EnemyHolder :
			child.respawn()