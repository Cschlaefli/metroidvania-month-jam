using Godot;
using System;
using Godot.Collections;

public static class Globals
{
    public const int CELL_SIZE =  256;
    public static Vector2 PlayerPositon = Vector2.Zero;
    [Signal]
    public delegate void NewScreen(Node screen);
    public static event NewScreen HandleNewScreen;
    public static void SetNewScreen(Screen screen)
    {
        HandleNewScreen(screen);
        CurrentScreen = screen;
    }
	public static Node Player { get; set; }
    public static Screen CurrentScreen { get; set; }
    public static Area CurrentArea { get; set; }
	public static Dictionary<String, object> SaveBuffer = new Dictionary<string, object>();
	public static Dictionary<String, object> LoadBuffer = new Dictionary<string, object>();
	public static int PlayerSpawnPosition;

	/*
    func change_area(new_area, position):
	#some transition screenfade here
	save_buffer[current_screen.name] = current_area._save()
	current_area.remove_child(player)
	current_area.queue_free()
	yield(current_area, "tree_exited")
	current_area = new_area
	new_area.debug = false
	get_tree().root.add_child(current_area)
	if save_buffer.has(new_area.name):
		new_area._load(save_buffer[new_area.name])
	elif load_buffer.has(new_area.name):
		new_area._load(load_buffer[new_area.name])

	player.global_position = new_area.spawn_points[position]
	new_area.add_child(player)
	*/

}
