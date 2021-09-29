using Godot;
using System;

public class SavePoint : Entrance
{
	bool active = false;
	Label Label;
	Particles2D particles;
    Tween Ctween;
    Gradient gradient;
    Color MidColor;
    Color ActivateColor = Colors.BlueViolet;


	public override void _Ready()
    {
		base._Ready();
		Label = GetNode<Label>("CanvasLayer/Label");
		particles = GetNode<Particles2D>("Particles2D");
        var pm = particles.ProcessMaterial as ParticlesMaterial;
        var gt = pm.ColorRamp as GradientTexture;
        gradient = gt.Gradient;
        Ctween = new Tween();
        AddChild(Ctween);
        Ctween.InterpolateMethod(this, "SetMidColor", gradient.GetColor(1), ActivateColor, .3f, Tween.TransitionType.Linear, Tween.EaseType.Out);
        Ctween.InterpolateMethod(this, "SetMidColor", ActivateColor, gradient.GetColor(1), .3f, Tween.TransitionType.Linear, Tween.EaseType.In, .3f);

    }
    public override void _Process(float delta)
    {
        base._Process(delta);
		var bods = GetOverlappingBodies();
		foreach(Node bod in bods)
        {
            var p = bod as Player;
            if(p != null)
            {
                active = true;
            }
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
    void SetMidColor(Color color)
    {
        gradient.SetColor(1, color);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if(active && @event.IsActionPressed("save"))
        {
            GD.Print("saving");
            Globals.Player.Health = Globals.Player.MaxHealth;
            Globals.Save(Globals.CurrentSave);
            if(!Ctween.IsActive())
            {
                Ctween.InterpolateMethod(this, "SetMidColor", gradient.GetColor(1), ActivateColor, .3f, Tween.TransitionType.Linear, Tween.EaseType.Out);
                Ctween.InterpolateMethod(this, "SetMidColor", ActivateColor, gradient.GetColor(1), .3f, Tween.TransitionType.Linear, Tween.EaseType.In, .3f);
                Ctween.Start();
            }
        }
    }
}
