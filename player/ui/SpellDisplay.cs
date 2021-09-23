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
    public void UpdateList(Array<Spell> sp)
    {
        foreach(Node c in GetChildren())
        {
            c.QueueFree();
        }
        var spellList = new Spell[sp.Count];

        if (sp.Count == 0) return;

        try
        {
            sp.CopyTo(spellList, 1);
            spellList[sp.Count - 1] = sp[0];
        }
        catch(Exception e)
        {
            GD.Print(e.Message);
        }

        foreach(int x in GD.Range(sp.Count))
        {
            var toAdd = new Sprite();
            var spell = spellList[x];
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
