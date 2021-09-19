extends Spell

export var charge_growth := .2

var apl_add = .4
var apl_base = .3
var by : KinematicBody2D
var dir : Vector2
onready var base_cost := casting_cost
var cost_add := 1.5

var charge_percent := 1.0


func _show_guide(delta):
	if not guide :
		$GuideParticles.visible = false
		return
	if charge_value > 1 :
		var temp = (charge_value)/(max_charge -1)
		var apl_lifetime = apl_base + apl_add * abs(temp)
		casting_cost = base_cost +  cost_add * abs(temp)
		$GuideParticles.lifetime = apl_lifetime
		$ActiveParticles.lifetime = apl_lifetime
		charge_percent = temp
	else:
		$GuideParticles.lifetime = apl_base
		$GuideParticles.restart()
		casting_cost = base_cost
	
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
	$GuideParticles.lifetime = apl_base
	$GuideParticles.restart()
	dir = direction
	self.by = by


func _on_activeTimer_timeout():
	if by != null  :
		by.velocity = -dir * ((5000 * charge_percent) + 1000)
	$Hurtbox.collision_mask = 0
	$ActiveParticles.emitting = false
	$Reflector.deactivate()
