extends Node2D

class_name EnemySpawner

export var respawn_screens := 2
var screen_changes = respawn_screens
var needs_reset := false

func _ready():
	Globals.connect("new_screen", self, "new_screen")

func player_entered():
	#activate enemies
	screen_changes = respawn_screens
	needs_reset = true
	for child in get_children() :
		if child is Enemy :
			child.wake()

func player_exited():
	#deactivate enemies
	for child in get_children() :
		if child is Enemy :
			child.sleep()

func new_screen(screen):
	#check if currenct screen
	screen_changes -=1
	if screen_changes == 0 :
		call_deferred("reset")

func reset():
	if needs_reset :
		for child in get_children() :
			if child is Enemy :
				child.respawn()
		needs_reset = false
