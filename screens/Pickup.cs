using Godot;
using System;

public class Pickup : Area2D, IPersist
{	
	Particles2D effect;
	Popup popup;
	protected Label label;
    public override void _Ready()
    {
        base._Ready();
		effect = GetNode<Particles2D>("Particles2D");
		popup = GetNode<Popup>("CanvasLayer/Popup");
		label = GetNode<Label>("CanvasLayer/Popup/Label");
		effect.Emitting = Persist;
		Connect("body_entered", this, nameof(OnAbilityPickupBodyEntered));
    }
    public bool Persist { get; set; }
	public virtual void AddAbility(Player player) { }

	protected async void OnAbilityPickupBodyEntered(KinematicBody2D body)
    {
		var p = body as Player;
		if(Persist && p != null)
        {
			AddAbility(p);
			Persist = false;
			p.UpdateSpells();
			effect.Emitting = false;
			popup.Show();
			await ToSignal(GetTree().CreateTimer(3.0f), "timeout");
			popup.Hide();
			
        }
    }
}
