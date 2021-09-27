using Godot;
using System;

public class CastInfo : Godot.Object
{
	public ICaster By { get; set; }
	public Vector2 Position { get; set; }
	public Vector2 Direction { get; set; }
}

public class Spell : Node2D
{
	[Export]
	public bool ShowEffect = true;
	[Export]
	public string SpellName = "Placeholder";
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
	protected float ProjectileDamage = 1;
	[Export]
	protected float ProjectileSpeed = 30;
	[Export]
	protected Vector2 Knockback = Vector2.Zero;
	[Export]
	protected float Recoil = 1000;
	[Export(PropertyHint.Layers2dPhysics)]
	protected uint Hitmask = 9;

	public bool Guide = false;
	public bool Current = false;
	public bool Casting = false;



	protected ProjectileInfo projectileInfo;

	[Export]
	public Texture MenuTexture;
	[Export]
	NodePath ProjectilePath = "Projectiles";
	[Export]
	public bool Interruptable = true;
	[Export]
	public bool Chargeable = false;
	public bool Charging = false;
	public bool CanCast = false;
	[Export]
	protected float MaxCharge = 20;
	protected float ChargeValue = 0;
	[Signal]
	public delegate void Updated();
	protected Node Projectiles;
	protected Timer ActiveTimer;
	protected Particles2D CastingEffect;
	public ShaderMaterial SpriteMat;
	protected Node projectiles;
	protected float ChargePercent = 0.0f;

	[Export]
	protected float ChargeCost = 0;
	[Export]
	protected float ChargeKnockback = 0;
	[Export]
	protected float ChargeDamage = 0;
	[Export]
	protected float ChargeSpeed = 0;
	[Export]
	protected float ChargeRecoil = 0;

	protected float BaseDamage;
	protected Vector2 BaseKnockback;
	protected float BaseCost;
	protected float BaseSpeed;
	protected float BaseRecoil;
	public bool IsActive => !ActiveTimer.IsStopped();


    public override void _Ready()
    {
        base._Ready();
		projectiles = GetNode<Node>(ProjectilePath);
		CastingEffect = GetNode<Particles2D>("CastingEffect");
		Projectiles = GetNode<Node>("Projectiles");
		ActiveTimer = GetNode<Timer>("ActiveTimer");
		ActiveTimer.Connect("timeout", this, nameof(OnActiveTimerTimeout));
		BaseCost = CastingCost;
		BaseKnockback = Knockback;
		BaseDamage = ProjectileDamage;
		BaseSpeed = ProjectileSpeed;
		BaseRecoil = Recoil;
    }

	public virtual void StartCasting(CastInfo ci)
    {
		Casting = true;
		CastingEffect.Emitting = ShowEffect;
    }

	public virtual void Interupt()
    {
		if( Interruptable)
        {
			Casting = false;
			GetNode<Particles2D>("CastingEffect").Emitting = false;
		}
    }
	
    public void StartCasting() => Casting = true;

	public virtual void Cast(CastInfo ci)
    {
		Casting = false;
		CastingEffect.Emitting = false;
        GetNode<Particles2D>("CastingEffect").Emitting = false;
		ActiveTimer.Start(ActiveTime);
		projectileInfo = new ProjectileInfo() {
			Damage = ProjectileDamage, Hitmask = Hitmask, HitstunTime = HitStun,
			ScaleValue = ChargePercent,
            Speed = ProjectileSpeed,
			Knockback = Knockback, Position = GlobalPosition,   };
	}

	public virtual void OnActiveTimerTimeout()
    {

    }
	public virtual void ShowGuide(float delta)
    {

    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
		if (Chargeable && Charging && ChargeValue <= MaxCharge)
        {
			ChargeValue += delta;
			ChargeValue = Math.Min(ChargeValue, MaxCharge);
        }
        else
        {
			ChargeValue = 1;
        }
		if(ChargeValue > 1)
        {
			ChargePercent = (ChargeValue - 1) / (MaxCharge - 1);
        }
		ShowGuide(delta);
		Update();
    }

}
