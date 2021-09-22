using Godot;
using System;

public class Screen : Node2D
{
	[Signal]
	delegate void PlayerExited();
	[Signal]
	delegate void PlayerEntered();
    public override void _Ready()
    {
        base._Ready();
		foreach(Node child in GetChildren())
        {
			if(child is Entrance)
            {
				child.Connect("PlayerEntered", this, "OnPlayerEntered");
            }
			if (child is EnemySpawner) {
				Connect("PlayerEntered", this, "OnPlayerEntered");
				Connect("PlayerExited", this, "OnPlayerExited");
			}
        }
    }

	public async void OnPlayerEntered(PlayerCamera camera, Vector2 transPosition)
    {
		if(Globals.CurrentScreen == this)
        {
			return;
        }else if (IsInstanceValid(Globals.CurrentScreen))
        {
			Globals.CurrentScreen.OnPlayerExited();
        }
		EmitSignal("PlayerEntered");
		Globals.SetNewScreen(this);
		var tileMap = GetNode<TileMap>("TileMap");
		var limit = tileMap.GetUsedRect().Size * tileMap.CellSize;
		var limits = new ScreenLimits(Position, limit);
		camera.Transition(transPosition, limits);
		await ToSignal(camera, nameof(PlayerCamera.EndTransition));
    }

	public void OnPlayerExited()
    {
		EmitSignal("PlayerExited");
    }
}
