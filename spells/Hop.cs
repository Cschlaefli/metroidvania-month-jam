using Godot;
using System;

public class Hop : Spell
{
    [Export]
    float Speed = 1500;
    [Export]
    float JumpHeight = 2;

    public override void StartCasting(CastInfo ci)
    {
        base.StartCasting(ci);
        var by = ci.By as ICaster;
        if(by != null)
        {
            by.Velocity = new Vector2(Math.Sign(ci.Direction.x) * Speed, by.Velocity.y);
        }
    }
    public override void Cast(CastInfo ci)
    {
        base.Cast(ci);
        var by = ci.By as ICaster;
        if (by != null)
        {
            var x = Math.Sign(ci.Direction.x) * Speed;
            var y = -Math.Sqrt(2 * by.Gravity * JumpHeight * Globals.CELL_SIZE * Globals.CELL_SIZE);
            by.Velocity = new Vector2(x, (float)y);
        }
    }

}
