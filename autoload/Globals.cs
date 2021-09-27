using Godot;
using System;
using Godot.Collections;
public static class Helpers
{

    public static float Accelerate(float v, float target, float accel, float delta)
        {
            if (Mathf.Round(v) == Mathf.Round(target))
            {
                v = target;
            }else if(v < target)
            {
                v += accel * delta;
                v = Mathf.Min(v, target);
            }else{
                v -= accel * delta;
                v = Mathf.Max(v, target);
            }
            return v;

        }
    public static Vector2 Accelerate(Vector2 v, Vector2 target, float accel, float delta)
    {
        var x = Accelerate(v.x, target.x, accel, delta);
        var y = Accelerate(v.y, target.y, accel, delta);
        return new Vector2(x, y);
    }

    public static float RandAngle()
    {
        return (GD.Randf() * (Mathf.Pi * 2)) - Mathf.Pi;
    }
}

public static class Globals
{
    public const int CELL_SIZE =  256;
	public static Player Player { get; set; }
    public static Area CurrentArea { get; set; }
	public static Dictionary<String, object> SaveBuffer = new Dictionary<string, object>();
	public static Dictionary<String, object> LoadBuffer = new Dictionary<string, object>();
	public static int PlayerSpawnPosition;
	public static bool MouseAim = true;
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
