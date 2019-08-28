extends Node2D

class_name Spell

export var casting_time := .1
export var casting_cost := 1.0
export var recovery_time := .1
export var known := false
export var equipped := false
var current := false
var casting := false

export(int) var hitmask = 9

signal updated

export var menu_tex : Texture
onready var projectiles = $Projectiles


func start_casting():
	casting = true
	$CastingEffect.emitting = true

func interupt():
	casting = false
	$CastingEffect.emitting = false

func cast(by : Node2D, point : Vector2 ,  direction : Vector2):
	casting = false
	$CastingEffect.emitting = false
	pass
