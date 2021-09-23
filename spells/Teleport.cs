using Godot;
using System;

public class Teleport : Spell
{
	[Export]
	public float Distance = Globals.CELL_SIZE * 4;

	RayCast2D LeftCheck;
	RayCast2D RightCheck;
	Area2D LeftBody;
	Area2D RightBody;

	float time = 0;

    public override void _Ready()
    {
        base._Ready();
        LeftCheck = GetNode<RayCast2D>("Left");
        RightCheck = GetNode<RayCast2D>("Right");
        LeftBody = GetNode<Area2D>("LeftBod");
        RightBody = GetNode<Area2D>("RightBod");

        LeftCheck.CastTo = Vector2.Left * Distance;
        RightCheck.CastTo = Vector2.Right * Distance;
        LeftBody.Position = LeftCheck.CastTo;
        RightBody.Position = RightCheck.CastTo;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (Casting)
        {
            time += 1;
            SpriteMat.SetShaderParam("amount", Mathf.Sin(time * 5) * 30);
        }
    }

    public override void Interupt()
    {
        base.Interupt();
        if (Interruptable)
        {
            SpriteMat.SetShaderParam("amount", 0.0f);
        }
    }

    public override void Cast(CastInfo ci)
    {
        if (!( ci.By is Player))
        {
            return;
        }
        SpriteMat.SetShaderParam("amount", 0);
        var dir = Mathf.Sign(ci.Direction.x);
        var dist = Distance;
        RayCast2D check;
        Area2D body;
        if(dir > 0)
        {
            check = RightCheck;
            body = RightBody;

        } else {  
            check = LeftCheck;
            body = LeftBody;
        }

        if (check.IsColliding())
        {
            //this is not checking overlaps if the raycast is hitting short;
            dist = check.GetCollisionPoint().DistanceTo(check.GlobalPosition);
            if( body.GetOverlappingBodies().Count == 0 && body.GetOverlappingAreas().Count == 0 ){
                var cam = Globals.Player.Cam;
                if (body.GlobalPosition.x < cam.screenLimits.Right && body.GlobalPosition.x > cam.screenLimits.Left && body.GlobalPosition.y > cam.screenLimits.Top && body.GlobalPosition.y < cam.screenLimits.Bottom)
                {
                    dist = Distance;
                }
            }

        }
        var x = ci.By.Velocity.x;
        var new_pos = new Vector2(ci.By.Position.x + dir * dist, ci.By.Position.y);

        ci.By.Velocity = new Vector2(x, 0);
        ci.By.Position = new_pos;

        base.Cast(ci);
    }
}
