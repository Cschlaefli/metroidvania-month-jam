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
	public static Dictionary SaveBuffer = new Dictionary();
	public static Dictionary LoadBuffer = new Dictionary();
	public static int PlayerSpawnPosition;
	public static bool MouseAim = true;
    public static string CurrentSave { get; set; }

    public static void SetCurrentSave(string file)
    {
        CurrentSave = file;
    }

    public static Dictionary Load(string fileName)
    {
        var retval = new Dictionary();
        fileName = $"user://{fileName}.json";

        using (File f = new File())
        {
            if (!f.FileExists(fileName))
            {
            }
            else
            {
                f.Open(fileName, File.ModeFlags.Read);
                var result = JSON.Parse(f.GetAsText());
                if (result.ErrorString != "")
                {
                    //GD.Print(f.GetAsText());
                    GD.Print(result.ErrorString);
                }
                else
                {
                    retval = result.Result as Dictionary;
                }
            }
        }
        return retval;
    }
    public static void SetLoadBuffer(string fileName)
    {
        LoadBuffer = Load(fileName);
    }

    public static void SetSaveBuffer()
    {
        SaveBuffer[CurrentArea.Name] = CurrentArea._Save();
        SaveBuffer["Player"] = Player.Save();
        SaveBuffer["CurrentArea"] = CurrentArea.Filename;
    }

    public static void Save(string fileName)
    {
        SetSaveBuffer();
        fileName = $"user://{fileName}.json";
        using (File f = new File())
        {
            f.Open(fileName, File.ModeFlags.Write);
            var sb = JSON.Print(SaveBuffer, "\t", true);
            f.StoreString(sb);
        }

    }
    public static void Delete(string fileName)
    {
        SaveBuffer.Clear();
        LoadBuffer.Clear();
        fileName = $"user://{fileName}.json";
        using (File f = new File())
        {
            f.Open(fileName, File.ModeFlags.Write);
            var sb = JSON.Print(SaveBuffer, "\t", true);
            f.StoreString(sb);
        }
    }

}
