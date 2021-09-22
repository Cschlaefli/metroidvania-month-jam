using Godot;
using System;

public class AreaExit : Area2D
{

	[Export(PropertyHint.File)]
	string NextArea;
	PackedScene Next;
	[Export]
	int PlayerPosition = 0;
	bool Active = false;

	[Signal]
	delegate void AreaChange(Area NewArea);

	public override void _Ready()
	{
		base._Ready();
		Connect("body_entered", this, nameof(OnBodyEntered));
		Connect("body_exited", this, nameof(OnBodyExited));
		if(NextArea != null)
		{
			Next = GD.Load<PackedScene>(NextArea);
		}
	}

	protected void OnBodyEntered(PhysicsBody2D body)
	{
		var p = body as Player;
		if(p != null) Active = true;
	}

	protected void OnBodyExited(PhysicsBody2D body)
	{
		var p = body as Player;
		if(p != null) Active = false;
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		if(Active && @event.IsActionPressed("ui_accept"))
		{
			if (Next == null)
			{
				return;
			}
			else
			{
				Globals.PlayerSpawnPosition = PlayerPosition;
				Globals.CurrentArea.LeaveTo(Next);
			}
		}
	}
}
