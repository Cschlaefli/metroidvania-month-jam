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

    /*

func new_screen(screen) :
	#check if currenct screen
	screen_changes -=1
	if screen_changes == 0 :
		call_deferred("reset")

func reset():
	if needs_reset :
		for child in get_children() :
			if child is EnemyExtra :
				child.respawn()
		needs_reset = false
	*/
}
