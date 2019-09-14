extends Spell

func _show_guide(delta):
	if not guide :
		$GuideParticles.visible = false
		return
	$GuideParticles.visible = true
	rotation = Globals.player.cast_dir.angle()

func cast(by : Node2D, point : Vector2 ,  direction : Vector2):
	rotation = Globals.player.cast_dir.angle()
	casting = false
	$CastingEffect.emitting = false
	$ActiveTimer.start(active_time)
	$Hurtbox.collision_mask = hitmask
	$Hurtbox.damage = projectile_damage
	$ActiveParticles.emitting = true
	$Reflector.activate()

func _on_activeTimer_timeout():
	$Hurtbox.collision_mask = 0
	$ActiveParticles.emitting = false
	$Reflector.deactivate()