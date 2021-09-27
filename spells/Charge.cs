using Godot;
using System;

public class Charge : Spell
{
    [Export]
    public float DrawBack = .5f;
    [Export]
    public float Distance = 4;
    public override void StartCasting(CastInfo ci)
    {
        base.StartCasting(ci);
        ci.By.Velocity = -ci.Direction * (DrawBack * Globals.CELL_SIZE) / CastingTime;
    }

    public override void Cast(CastInfo ci)
    {
        ci.By.Velocity = ci.Direction * (Distance * Globals.CELL_SIZE) / ActiveTime;
        base.Cast(ci);
    }
    /*

    var dir : Vector2
    var by : Enemy
    export var distance := 4.0
    export var draw_back := .5
    func start_casting():
        casting = true
        by.velocity = -dir * (draw_back * Globals.CELL_SIZE)/casting_time

    func interupt():
        casting = false
        $CastingEffect.emitting = false

    func cast(by : Node2D, point : Vector2 ,  direction : Vector2):
        by.velocity = dir * (distance * Globals.CELL_SIZE)/casting_time
        casting = false
        $CastingEffect.emitting = false
        pass
    */
}
