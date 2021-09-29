using Godot;
using System;

public class EnemySpawner : Node
{
	[Export]
	public int RespawnScreens = 2;
	public int ScreenChanges;
	public bool NeedsReset = false;
	public Screen ContainingScreen;
    public override void _Ready()
    {
        base._Ready();
		ContainingScreen = GetParentOrNull<Screen>();
    }
	public void OnPlayerEntered()
    {
		ScreenChanges = RespawnScreens;
		NeedsReset = true;
		foreach(Enemy child in GetChildren())
        {
			child.Wake();
        }
    }
	public void OnPlayerExited()
    {
		foreach(Enemy child in GetChildren())
        {
			child.Sleep();
        }
    }
	public void NewScreen(Node screen)
    {
		ScreenChanges -= 1;
		if(ScreenChanges < 1) {
			CallDeferred("Reset");
		}
    }
	void Reset()
    {
        if (NeedsReset)
        {
			foreach(Enemy child in GetChildren())
            {
				child.Respawn();
            }
        }
    }

}
