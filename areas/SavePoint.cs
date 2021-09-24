using Godot;
using System;

public class SavePoint : Entrance
{
	bool active = false;
	Label Label;
	Particles2D particles;
	public override void _Ready()
    {
		base._Ready();
		Label = GetNode<Label>("CanvasLayer/Label");
		particles = GetNode<Particles2D>("Particles2D");
    }
    public override void _Process(float delta)
    {
        base._Process(delta);
		var bods = GetOverlappingBodies();
		foreach(Player bod in bods)
        {
			active = true;
        }
        var pm = particles.ProcessMaterial as ParticlesMaterial;
        if (active)
        {
			Label.Visible = true;
			pm.InitialVelocity = 300;
        }
        else
        {
			Label.Visible = false;
			pm.InitialVelocity = 100;
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if(active && @event.IsActionPressed("ui_accept"))
        {
            Globals.Player.Health = Globals.Player.MaxHealth;
            //Globals.Save();
            //adjust color here;
        }
    }
    /*


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

     */
}
