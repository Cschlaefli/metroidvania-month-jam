using Godot;
using System;

public class Staff : Node2D
{
	public Position2D ProjectileSpawnPosition;
    public override void _Ready()
    {
        base._Ready();
		ProjectileSpawnPosition = GetNode<Position2D>("ProjectilePosition");
    }
  
}
