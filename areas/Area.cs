using Godot;
using Godot.Collections;
using System;

public interface IPersist
{
    bool Persist { get; set; }
}

public class Area : Node2D
{
	public bool debug = true;
    [Export]
    public int SpawnAt = 0;
    Array<Vector2> SpawnPoints = new Array<Vector2>();
    [Signal]
    public delegate void NewScreen(Screen screen);
    public Screen CurrentScreen;


    public void SetNewScreen(Screen screen)
    {
        CurrentScreen = screen;
        EmitSignal("NewScreen", screen);
    }

    public override void _Ready()
    {
        base._Ready();


        var spawns = GetNode<Node>("Spawns");
        foreach (Node2D spawn in spawns.GetChildren())
        {
            SpawnPoints.Add(spawn.GlobalPosition);
        }

        if (Globals.SaveBuffer.Contains(Name))
        {
            debug = false;
            var save = Globals.SaveBuffer[Name] as Dictionary<string, bool>;
            if (save != null)
            {
                _Load(save);
            }
            var p = GD.Load<PackedScene>("res://player/Player.tscn");
            var pl = p.Instance<Player>();
            AddChild(pl);
            Globals.Player.Load(Globals.SaveBuffer["Player"] as Dictionary);
        }
        else if (Globals.LoadBuffer.Contains(Name))
        {
            debug = false;
            var save = Globals.LoadBuffer[Name] as Dictionary<string, bool>;
            if (save != null)
            {
                _Load(save);
            }
            var p = GD.Load<PackedScene>("res://player/Player.tscn");
            var pl = p.Instance<Player>();
            AddChild(pl);
            Globals.Player.Load(Globals.LoadBuffer["Player"] as Dictionary);
        }

        if (Globals.Player != null && IsInstanceValid(Globals.Player))
        {
            debug = false;
        }
        else
        {
            var p = GD.Load<PackedScene>("res://player/Player.tscn");
            var pl = p.Instance<Player>();
            pl.GlobalPosition = SpawnPoints[SpawnAt];
            AddChild(pl);
            if(Globals.LoadBuffer.Contains("Player"))
            {
                Globals.Player.Load(Globals.LoadBuffer["Player"] as Dictionary);
            }
        }

        AddToGroup("area");
        Globals.CurrentArea = this;
    }
    public Dictionary _Save()
    {
        var SaveNodes = GetTree().GetNodesInGroup("persist");
        var dict = new Dictionary();
        foreach(Node n in SaveNodes)
        {
            var p = n as IPersist;
            if (p != null)
            {
                dict[GetPathTo(n)] = p.Persist;
            }
        }
        return dict;
    }

    public void _Load(Dictionary<string, bool> dict)
    {
        foreach(string np in dict.Keys)
        {
            var per = GetNode<IPersist>(np);
            var val = dict[np];
            per.Persist = val;
        }
        if (Globals.Player != null && IsInstanceValid(Globals.Player)) {
            Globals.Player.QueueFree();
        }
    }

    public void LoadSave()
    {
        var dict = Globals.LoadBuffer;
        var currArea = dict["CurrentArea"] as string;
        var newArea  = GD.Load<PackedScene>(currArea);
        LeaveTo(newArea);

    }

    public void LeaveTo(PackedScene NewArea)
    {
        Globals.SaveBuffer[Name] = _Save();
        //RemoveChild(Globals.Player);
        Globals.Player.QueueFree();
        GetTree().ChangeSceneTo(NewArea);
    }
}
