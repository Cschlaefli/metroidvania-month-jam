using Godot;
using System;
using Godot.Collections;

public class SpellDisplay : Node2D
{
	int offset = 64;
	Tween tween;

    public override void _Ready()
    {
        base._Ready();
        tween = new Tween();
        AddChild(tween);
    }
    public void UpdateList(Array<Spell> spells)
    {
        var sp = spells.Duplicate();
        foreach(Node c in GetChildren())
        {
            c.QueueFree();
        }

        if (sp.Count == 0) return;
        if (sp.Count == 1)
        {
            sp.Insert(0, sp[0]);
            sp.Insert(0, sp[0]);
        }
        else if(sp.Count >= 2)
        {
            sp.Insert(0, sp[sp.Count - 1]);
        }



        foreach(int x in GD.Range(sp.Count-1))
        {
            var toAdd = new Sprite();
            var spell = sp[x];
            toAdd.Texture = spell.MenuTexture;
            toAdd.Position = new Vector2(offset * x, Position.y);
            if (x != 1)
            {
                toAdd.Scale  *= .75f;
                var c = toAdd.Modulate;
                c.a = .5f;
                toAdd.Modulate = c;
            }else if(x >= 3)
            {

                toAdd.Scale  *= .75f;
                var c = toAdd.Modulate;
                c.a = .0f;
                toAdd.Modulate = c;
            }
            AddChild(toAdd);

        }

    }
}
