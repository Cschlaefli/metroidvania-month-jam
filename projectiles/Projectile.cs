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
	public int max_reflects {get;set;}= 1 << 12;
	[Export]
	public bool explodes { get; } = false;
	[Export]
	private bool dissolves = true;
	[Export]
	private Damage.dam_types effect = 0;

	public Vector2 direction = Vector2.Zero;
	public int reflect_count {get;set;} = 0;

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
			hitable.Hit(this, damage, effect, knockback, hitstun);
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
		if (!direction.IsNormalized()) {
			direction = direction.Normalized();
		}
		this.Position += direction * speed;
	}
	public void reflect(uint new_hitmask, Vector2 new_direction)
	{
		reflect_count += 1;
		if (reflect_count >= max_reflects) {
			_explode();
		}else{
			this.CollisionMask = new_hitmask;
			direction = direction.Reflect(new_direction);
			this.Rotation = direction.Angle();
		}
	}
	public virtual void _explode()
	{}

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

