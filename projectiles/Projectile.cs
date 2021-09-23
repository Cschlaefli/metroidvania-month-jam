using Godot;
using System;

public class Projectile : Area2D, IReflectable, IExplodes
{

	[Export]
	private float speed = 5.0f;
	[Export]
	private float damage = 1.0f;
	[Export]
	private Vector2 knockback = Vector2.Zero;
	[Export]
	private float hitstun = .2f;
	[Export]
	private bool reflectable = true;
	[Export]
	public int MaxReflects {get;set;}= 1 << 12;
	[Export]
	public bool explodes { get; set; } = false;
	[Export]
	private bool dissolves = true;
	[Export(PropertyHint.Flags)]
	private Damage.dam_types effect = 0;

	public Vector2 Direction { get; set; } = Vector2.Zero;
	public int ReflectsCount {get;set;} = 0;


	private CPUParticles2D particles;
	private Timer dissolve_timer;
	
	public override void _Ready()
	{
		particles = GetNode<CPUParticles2D>("CPUParticles2D");
		dissolve_timer = GetNode<Timer>("DissolveTimer");
	}
	public void _on_Projectile_body_entered(Node2D body){
		var terrain = body as TileMap;
		var hitable = body as IHitbox;

        if (hitable != null)
        {
			var hi = new HitInfo(this, damage, effect, knockback, hitstun);
			hitable.Hit(hi);
			if (dissolves) _dissolve();
        }
				
		if (terrain != null){
			
			if (dissolves){
				_dissolve();
			}
		}
	}

	public override void _PhysicsProcess(float delta)
	{
		if (!Direction.IsNormalized()) {
			Direction = Direction.Normalized();
		}
		this.Position += Direction * speed;
	}
	public void Reflect(uint new_hitmask, Vector2 new_direction, float speedMod, float damageMod)
	{
		damage *= damageMod;
		speed *= speedMod;
		ReflectsCount += 1;
		if (ReflectsCount >= ReflectsCount || !reflectable) {
			_explode();
		}else{
			this.CollisionMask = new_hitmask;
			Direction = Direction.Reflect(new_direction);
			this.Rotation = Direction.Angle();
		}
	}
	public virtual void _explode()
	{
		_dissolve();
	}

	public virtual void _dissolve()
	{
		if (dissolve_timer.IsStopped()){
			this.CollisionMask = 0;
			speed = 12.5f;
			particles.Emitting = false;
			dissolve_timer.Start();
		}
	}

	public void _on_DissolveTimer_timeout()
	{
		QueueFree();
	}
}

