using Godot;
using System;
using Godot.Collections;

public class SpellBarrage : Node2D
{
	bool Active = false;
	[Export]
	float InitalDelay = 4;
	[Export]
	float ShotSpacing = .5f;
	[Export]
	float BarrageDelay = 4;
	[Export]
	bool RandomOrder = false;

	ICaster Caster;

	float Time = 0;
	Array<Spell> Spells = new Array<Spell>();

	public void Activate(float delay = 0)
    {
		if(delay == 0)
        {
            delay = InitalDelay;
        }
		Time = delay;
		Active = true;
    }

	public void Deactivate()
    {
		Time = 0;
		Active = false;
    }

    public override void _Ready()
    {
        base._Ready();
		SetSpells();
		var parent = GetParent();
		Caster = parent as ICaster;
		while(Caster == null)
        {
			parent = parent.GetParent();
			Caster = parent as ICaster;
        }
    }

	void SetSpells()
    {
		Spells = new Array<Spell>();
		foreach(var child in GetChildren())
        {
			var sp = child as Spell;
			if(sp != null)
            {
                Spells.Add(sp);
            }
        }
		if (RandomOrder) Spells.Shuffle();
		GD.Print(Spells.Count);
    }

	public float FireNext()
    {
		if(Spells.Count == 0)
        {
			SetSpells();
			return BarrageDelay;
        }
        else
        {
			var spell = Spells[0];
			Spells.RemoveAt(0);
			var playerDirection = Globals.Player.GlobalPosition - spell.GlobalPosition;
			var ci = new CastInfo() { By = Caster, Direction = playerDirection, Position = GlobalPosition };
			spell.Cast(ci);
			return ShotSpacing;
        }
    }
	public void FireAll()
    {
		SetSpells();
		foreach(var spell in Spells)
        {
			var playerDirection = Globals.Player.GlobalPosition - spell.GlobalPosition;
			var ci = new CastInfo() { By = Caster, Direction = playerDirection, Position = GlobalPosition };
			spell.Cast(ci);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (Active)
        {
			Time -= delta;
			if(Time <= 0)
            {
				Time = FireNext();
            }
        }
    }

}
