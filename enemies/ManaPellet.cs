using Godot;
using System;

public class ManaPellet : KinematicBody2D
{
    public Vector2 Velocity = Vector2.Zero;
    public float Speed = Globals.CELL_SIZE * 10;
    public float Amount = 5;

    Area2D DetectionArea;
    Area2D Pellet;
    Timer DissolveTimer;
    CPUParticles2D vfx;

    public override void _Ready()
    {
        DetectionArea = GetNode<Area2D>("DetectionArea");
        Pellet = GetNode<Area2D>("Pellet");
        DissolveTimer = GetNode<Timer>("DissolveTimer");
        vfx = GetNode<CPUParticles2D>("CPUParticles2D");
        DissolveTimer.Connect("timeout", this, nameof(_OnDissolveTimerTimeout));
        Pellet.Connect("body_entered", this, nameof(_OnPelletBodyEntered));
        base._Ready();
        vfx.Lifetime += Amount * .1f;
        vfx.SpeedScale = Amount * Amount;
    }

    public override void _PhysicsProcess(float delta)
    {
        var targetVelocity = Vector2.Zero;
        if( DetectionArea.GetOverlappingBodies().Count > 0)
        {
            targetVelocity = (Globals.Player.GlobalPosition - GlobalPosition).Normalized() * Speed;
            Velocity = Velocity.LinearInterpolate(targetVelocity, delta * 5);
        }
        else
        {
            targetVelocity = (Globals.Player.GlobalPosition - GlobalPosition).Normalized() * Speed;
            Velocity = Velocity.LinearInterpolate(targetVelocity, delta);
        }
        MoveAndSlide(Velocity);
    }

    public void _OnPelletBodyEntered(Node body)
    {
        var p = body as Player;
        if(p != null)
        {
            p.Mana += Amount;
            Amount = 0;
        }
        _Dissolve();
    }
    public void _Dissolve()
    {
        DissolveTimer.Start();
        vfx.Lifetime = .2f;
        vfx.Emitting = false;
    }
    public void _OnDissolveTimerTimeout()
    {
        QueueFree();
    }

}
