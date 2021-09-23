using Godot;
using System;
using Godot.Collections;

public class SpellEquipMenu : Control
{
	Node2D Spells;
	public Godot.Collections.Array PlayerSpells;
	Array<Spell> DisplayedSpells;
	PackedScene bntSpellToggle;
    public override void _Ready()
    {
        base._Ready();
		Spells = GetNode<Node2D>("Spells");
		bntSpellToggle = GD.Load<PackedScene>("res://player/ui/SpellToggleButton.tscn");
		DisplayedSpells = new Array<Spell>();

    }
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event.IsActionPressed("Pause"))
        {
			UpdateDisplay();
			Toggle();
        }
    }
	public void Display()
    {
		var sps = new Array<SpellToggleButton>();
		foreach(Spell spell in DisplayedSpells)
        {
			var toAdd = bntSpellToggle.Instance<SpellToggleButton>();
			Spells.AddChild(toAdd);
			sps.Add(toAdd);
        }
		if (sps.Count == 0 ) return;
		sps[0].FocusNeighbourLeft = sps[sps.Count-1].GetPath();
		sps[sps.Count -1].FocusNeighbourLeft = sps[0].GetPath();

    }
	public void UpdateDisplay()
    {
		DisplayedSpells = new Array<Spell>();
		foreach(Node c in GetChildren())
        {
			c.QueueFree();
        }
		foreach(Spell s in PlayerSpells)
        {
            if (s.Known)
            {
				DisplayedSpells.Add(s);
            }
        }
		Display();
    }

	public void Toggle()
    {
		Visible = !Visible;
		if (Visible)
		{
			var sp = Spells.GetChildOrNull<SpellToggleButton>(0);
			sp.GrabFocus();
			GetTree().Paused = true;
		}
        else
        {
			GetTree().Paused = false;
        }
    }
}
