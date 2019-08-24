extends Area2D
tool

class_name Entrance

export var camera_offset := Vector2( 512, -512)
onready var camera_transition_pos = $CameraOffset

signal player_entered

func _ready():
	$CameraOffset.position = camera_offset

func _on_body_entered(body) :
	if body is Player :
		emit_signal("player_entered", body.cam, camera_transition_pos.global_position)

func _process(delta):
	if Engine.editor_hint :
		$CameraOffset.position = camera_offset
