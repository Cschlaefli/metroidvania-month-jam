using Godot;
using System;
using Godot.Collections;

public static class SpellList
{
    static Array<PackedScene> _ps = new Array<PackedScene>();
    public static Array<PackedScene> PlayerSpells
    {
        get {
            if(_ps.Count <= 0)
            {
                /*
                 * preload("res://spells/BasicSpell.tscn"),
                    preload('res://spells/FearSpell.tscn'),
                    preload("res://spells/Spell.tscn"),
                    preload('res://spells/IceBurstSpell.tscn'),
                    preload('res://spells/SonicBoomSpell.tscn'),
                    preload('res://spells/BlastSpell.tscn'),
                 */
                _ps.Add(GD.Load<PackedScene>("res://spells/BasicSpell.tscn"));
                _ps.Add(GD.Load<PackedScene>("res://spells/FearSpell.tscn"));
                _ps.Add(GD.Load<PackedScene>("res://spells/Spell.tscn"));
                _ps.Add(GD.Load<PackedScene>("res://spells/IceBurstSpell.tscn"));
                _ps.Add(GD.Load<PackedScene>("res://spells/SonicBoomSpell.tscn"));
                _ps.Add(GD.Load<PackedScene>("res://spells/BlastSpell.tscn"));
            }
            return _ps;
        }
    }
    


}
