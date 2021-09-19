extends Entrance
var active := false


func _process(delta):
	var bods = get_overlapping_bodies()
	active = false
	for bod in bods :
		if bod is Player :
			active = true
	if active :
		$CanvasLayer/Label.visible = true
		$Particles2D.process_material.initial_velocity = 300.0
	else :
		$CanvasLayer/Label.visible = false
		$Particles2D.process_material.initial_velocity = 100.0

onready var gradient : Gradient = $Particles2D.process_material.color_ramp.gradient
onready var mid_color := gradient.get_color(1)

onready var c_tween := Tween.new()
export var activate_color := Color.blueviolet

func _ready():
	activate_color.a = .3
	add_child(c_tween)
	c_tween.interpolate_method(self, "set_mid_color", gradient.get_color(1), activate_color, .3, Tween.TRANS_LINEAR, Tween.EASE_OUT)
	c_tween.interpolate_method(self, "set_mid_color",  activate_color, gradient.get_color(1), .3, Tween.TRANS_LINEAR, Tween.EASE_IN, .3)

func set_mid_color(color : Color):
	gradient.set_color(1,color)

func _input(event):
	if active  and event.is_action_pressed("ui_accept") :
		Globals.player.health = Globals.player.max_health
		Globals.save()
		if not c_tween.is_active() :
			c_tween.interpolate_method(self, "set_mid_color", gradient.get_color(1), activate_color, .3, Tween.TRANS_LINEAR, Tween.EASE_OUT)
			c_tween.interpolate_method(self, "set_mid_color",  activate_color, gradient.get_color(1), .3, Tween.TRANS_LINEAR, Tween.EASE_IN, .3)
			c_tween.start()
