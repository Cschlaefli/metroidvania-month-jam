using Godot;
using System;

public class SpellToggleButton : TextureButton
{
	Spell spell;
    public override void _Ready()
    {
        base._Ready();
		TextureNormal = spell.MenuTexture;
		GetNode<Label>("Info/SpellCost").Text = $"Cost : {spell.CastingCost}";
		GetNode<Label>("Info/SpellName").Text = $"{spell.SpellName}";
		SelfModulate = new Color(SelfModulate, .4f);
        Connect("focus_entered", this, nameof(OnFocusEntered));
        Connect("focus_exited", this, nameof(OnFocusExited));
    }
    public override void _Pressed()
    {
        base._Pressed();
		spell.Equipped = !spell.Equipped;
		spell.EmitSignal(nameof(Spell.Updated));
		_Update();
    }
	void _Update()
    {
        if (spell.Equipped)
        {
			SelfModulate = new Color(.8f, .8f, .8f, 1.0f);
        }
        else
        {
			SelfModulate = new Color(.2f, .2f, .2f, 1.0f);
        }
    }
    public void OnFocusEntered() =>
        SelfModulate = new Color(SelfModulate, 1.0f);
    public void OnFocusExited() =>
        SelfModulate = new Color(SelfModulate, .6f);
}
