using Godot;
using System;

public class CastInfo 
{
	public Node2D By { get; set; }
	public Vector2 Position { get; set; }
	public Vector2 Direction { get; set; }
}

public class Spell : Node2D
{
	[Export]
	public float CastingTime = .1f;
	[Export]
	public float CastingCost = .1f;
	[Export]
	public float RecoveryTime = .1f;
	[Export]
	public bool Known = false;
	[Export]
	public bool Equipped = false;
	[Export]
	public float ActiveTime = .1f;
	[Export]
	public float HitStun = .3f;
	[Export]
	float ProjectileDamage = 1;
	[Export]
	float ProjectilesSpeed = 30;
	[Export]
	float Recoil = 1000;
	[Export]
	bool LooseCasting = false;

	bool Guide = false;
	bool Current = false;
	bool Casting = false;

	[Export(PropertyHint.Flags)]
	int hitmask = 9;

	[Export]
	Texture MenuTexture;
	[Export]
	NodePath ProjectilePath = "Projectiles";
	[Export]
	public bool Interruptable = true;
	[Export]
	public bool Chargeable = false;
	bool Charging = false;
	[Export]
	float MaxCharge = 20;
	float ChargeValue = 0;
	[Signal]
	delegate void Updated();
	Node Projectiles;
	Timer ActiveTimer;

    public override void _Ready()
    {
        base._Ready();
		Projectiles = GetNode<Node>("Projectiles");
		ActiveTimer = GetNode<Timer>("ActiveTimer");
    }

	public virtual void StartCasting()
    {
		Casting = true;
    }

	public virtual void Interupt()
    {
		if( Interruptable)
        {
			Casting = false;
			GetNode<Particles2D>("CastingEffect").Emitting = false;
		}
    }
	
	public virtual void Cast(CastInfo ci)
    {
		Casting = false;
        GetNode<Particles2D>("CastingEffect").Emitting = false;
		ActiveTimer.Start(ActiveTime);
	}

	public virtual void _OnActiveTimerTimeout()
    {

    }
	public virtual void ShowGuide(float delta)
    {

    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
		if (Chargeable && Charging && ChargeValue < MaxCharge)
        {
			ChargeValue += delta;
			ChargeValue = Math.Min(ChargeValue, MaxCharge);
        }
        else
        {
			ChargeValue = 1;
        }
		ShowGuide(delta);
		Update();
    }

}
