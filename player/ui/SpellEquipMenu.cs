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

    /*
    extends Control

var spells
var displayed_spells = []
var spell_toggle = preload("res://player/ui/SpellToggleButton.tscn")

func _input(event):
	if event.is_action_pressed("pause") :
		update_display()
		toggle()

func update_display():
	displayed_spells = []
	for child in $Spells.get_children() : child.free()
	for spell in spells :
		if spell.known :
			displayed_spells.append(spell)
	_display()

func _display():
	var sps = []
	for spell in displayed_spells :
		var to_add = spell_toggle.instance()
		to_add.spell = spell
		$Spells.add_child(to_add)
		sps.append(to_add)
	if sps.size() == 0 :return
	sps.back().focus_neighbour_right = sps.front().get_path()
	sps.front().focus_neighbour_left = sps.back().get_path()


func toggle():
	visible = !visible
	if visible :
		var sp = $Spells.get_children().front()
		if sp : sp.grab_focus()
		get_tree().paused = true
	else:
		get_tree().paused = false
	*/

}
