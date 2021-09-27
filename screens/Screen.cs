using Godot;
using System;

public class Screen : Node2D
{
	[Signal]
	delegate void PlayerExited();
	[Signal]
	delegate void PlayerEntered();
	public ScreenLimits screenLimits;
	public override void _Ready()
    {
        base._Ready();
		var tileMap = GetNode<TileMap>("TileMap");
		var limit = tileMap.GetUsedRect().Size * tileMap.CellSize;
		screenLimits = new ScreenLimits(Position, limit);

		foreach(Node child in GetChildren())
        {
			var ent = child as Entrance;
			var es = child as EnemySpawner;
			if(ent != null)
            {
				child.Connect(nameof(PlayerEntered), this, nameof(OnPlayerEntered));
            }
			if (es != null) {
				Connect(nameof(PlayerEntered), es, nameof(EnemySpawner.OnPlayerEntered));
				Connect(nameof(PlayerExited), es, nameof(EnemySpawner.OnPlayerExited));
				var Area = GetParent<Area>();
				Area.Connect(nameof(Area.NewScreen), es, nameof(EnemySpawner.NewScreen));
			}
        }
    }

	public async void OnPlayerEntered(PlayerCamera camera, Vector2 transPosition)
    {
		if(Globals.CurrentArea.CurrentScreen == this)
        {
			return;
        }else if (IsInstanceValid(Globals.CurrentArea.CurrentScreen))
        {
			Globals.CurrentArea.CurrentScreen.OnPlayerExited();
        }
		EmitSignal("PlayerEntered");
		Globals.CurrentArea.SetNewScreen(this);
		camera.Transition(transPosition, screenLimits);
		await ToSignal(camera, nameof(PlayerCamera.EndTransition));
    }

	public void OnPlayerExited()
    {
		EmitSignal("PlayerExited");
    }
}
