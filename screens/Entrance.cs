using Godot;
using System;
[Tool]
public class Entrance : Area2D
{
	[Export]
	Vector2 CameraOffset = new Vector2(512, -512);
	Position2D CameraTransitionPos;
    [Signal]
    delegate void PlayerEntered(PlayerCamera playerCamera, Vector2 position );
    public override void _Ready()
    {
        base._Ready();
        CameraTransitionPos = GetNode<Position2D>("CameraOffset");
        Connect("on_body_entered", this, nameof(OnBodyEntered));
    }
    protected void OnBodyEntered(PhysicsBody2D body)
    {
        var p = body as Player;
        if(p != null)
        {
            EmitSignal("PlayerEntered", p.Cam, CameraTransitionPos.GlobalPosition);
        }
    }
    public override void _Process(float delta)
    {
        if (Engine.EditorHint)
        {
            CameraTransitionPos.Position = CameraOffset;
        }
        base._Process(delta);
    }
}
