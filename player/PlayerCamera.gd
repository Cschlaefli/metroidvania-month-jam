extends Camera2D

var trans := false
var trans_pos : Vector2
var trans_zoom  : Vector2
signal transitioning
signal end_trans

onready var current_limits = { 'top' : limit_top, 'left' : limit_left, 'right' : limit_right, 'bottom' : limit_bottom}

func transition(pos, new_limits, new_zoom := 5):
	$SoftLockPreventer.start()
	emit_signal("transitioning")
	trans = true
	trans_pos = pos
	trans_zoom = new_zoom*Vector2.ONE
	current_limits.top = min(new_limits.top, current_limits.top)
	current_limits.right = max(new_limits.right, current_limits.right)
	current_limits.bottom = max(new_limits.bottom, current_limits.bottom)
	current_limits.left = min(new_limits.left, current_limits.left)

	_set_limits(current_limits)
	current_limits = new_limits
	_set_drag_margins(0.0)
	smoothing_enabled = false
	get_tree().paused = true

func end_transition():
	zoom = trans_zoom
	trans = false
	smoothing_enabled = true
	_set_limits(current_limits)
	_set_drag_margins(0.01)
	emit_signal("end_trans")
	get_tree().paused = false

func _physics_process(delta):
	if trans :
		var pos = global_position
		if (pos.round() != trans_pos.round() or zoom.round() != trans_zoom.round()) and not $SoftLockPreventer.is_stopped()  :
			offset = lerp(offset, Vector2.ZERO, delta * 10)
			global_position = lerp(pos, trans_pos, delta * 20)
			zoom = lerp(zoom, trans_zoom, delta * 5)
		else :
			end_transition()


func _set_drag_margins(v : float):
	drag_margin_bottom = v
	drag_margin_top = v
	drag_margin_right = v
	drag_margin_left = v

func _set_limits(margins):
	limit_top = margins.top
	limit_bottom = margins.bottom
	limit_left = margins.left
	limit_right = margins.right
