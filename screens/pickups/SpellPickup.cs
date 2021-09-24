using Godot;
using System;

public class SpellPickup : Pickup
{
    Spell _spell;
    [Export]
    PackedScene Spell;

    public override void _Ready()
    {
        base._Ready();
        _spell = Spell.Instance<Spell>();
    }
    public override void AddAbility(Player player)
    {
        base.AddAbility(player);
        foreach(Spell sp in player.SpellNode.GetChildren())
        {
            if(sp.SpellName == _spell.SpellName)
            {
                sp.Known = true;
                sp.Equipped = true;
                label.Text = "New spell " + sp.SpellName + " learned \n Aim with mouse, cast with lmb, press tab to select active spells and q + e to cycle through active spells";
            }
        }
    }
}
