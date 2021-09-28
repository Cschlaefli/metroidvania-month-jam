using Godot;
using System;
using Godot.Collections;

public class SpellEquipMenu : Control
{
	Control Spells;
	public Godot.Collections.Array PlayerSpells;
	Array<Spell> DisplayedSpells;
	PackedScene bntSpellToggle;
    public override void _Ready()
    {
        base._Ready();
		Spells = GetNode<Control>("Spells");
		bntSpellToggle = GD.Load<PackedScene>("res://player/ui/SpellToggleButton.tscn");
		DisplayedSpells = new Array<Spell>();

    }
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event.IsActionPressed("pause"))
        {
			UpdateDisplay();
			Toggle();
        }
    }
	public void Display()
    {
		var sps = new Array<SpellToggleButton>();
		foreach(Node n in DisplayedSpells)
        {
			var sp = n as Spell;
			if (sp != null) {
                var toAdd = bntSpellToggle.Instance<SpellToggleButton>();
                toAdd.spell = sp;
                Spells.AddChild(toAdd);
                sps.Add(toAdd);
			}
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
		foreach(Spell s in Globals.Player.SpellNode.GetChildren())
        {
            if ( s != null && s.Known)
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
			if(sp != null)
            {
                sp.GrabFocus();
            }
			GetTree().Paused = true;
		}
        else
        {
			GetTree().Paused = false;
        }
    }
}
