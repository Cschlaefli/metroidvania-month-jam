using Godot;
using System;
[Tool]
public class Entrance : Area2D
{
	[Export]
	Vector2 CameraOffset = new Vector2(512, -512);
	Position2D CameraTransitionPos;
    public override void _Ready()
    {
        base._Ready();
        CameraTransitionPos = GetNode<Position2D>("CameraOffset");
    }
    protected void OnBodyEntered(PhysicsBody2D body)
    {
        if(body is KinematicBody2D)
        {
            //EmitSignal("PlayerEntered", body.Camera, CameraTransitionPosition.GlobalPosition);
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
