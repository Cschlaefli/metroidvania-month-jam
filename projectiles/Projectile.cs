using Godot;
using System;

public class ProjectileInfo : Godot.Object
{
    [Export]
	public float Damage = 1;
	[Export]
	public float Speed = 30;
	[Export]
	public float HitstunTime = .1f;
	[Export]
	public Vector2 Knockback = Vector2.Zero;
	[Export(PropertyHint.Layers2dPhysics)]
	public uint Hitmask = 9;
	[Export]
	public Vector2 Position = Vector2.Zero;
	[Export]
	public float ScaleValue = 0;

}

public class Projectile : Area2D, IReflectable, IExplodes
{

	[Export]
	private float Speed = 5.0f;
	[Export]
	private float Damage = 1.0f;
	[Export]
	private Vector2 Knockback = Vector2.Zero;
	[Export]
	private float Hitstun = .2f;
	[Export]
	private bool Reflectable = true;
	[Export]
	public int MaxReflects {get;set;}= 1 << 12;
	[Export]
	public bool explodes { get; set; } = false;
	[Export]
	private bool dissolves = true;
	[Export(PropertyHint.Flags)]
	private Damage.dam_types Effect = 0;

	public Vector2 Direction { get; set; } = Vector2.Zero;
	public int ReflectsCount {get;set;} = 0;

	public float ScaleValue = .0f;
	[Export]
	CapsuleShape2D ScaledObject;
	[Export]
	float ScaledRadius = 100;
	CapsuleShape2D CapsuleShape;

	private CPUParticles2D particles;
	private Timer DissolveTimer;
	
	public override void _Ready()
	{
		particles = GetNode<CPUParticles2D>("CPUParticles2D");
		DissolveTimer = GetNode<Timer>("DissolveTimer");
		particles.EmissionSphereRadius = Mathf.Lerp(particles.EmissionSphereRadius, ScaledRadius, ScaleValue);
		var shape = GetNode<CollisionShape2D>("CollisionShape2D").Shape;
		CapsuleShape = shape as CapsuleShape2D;
		if(CapsuleShape != null) 
		{
            var val = Mathf.Lerp(CapsuleShape.Radius, ScaledRadius, ScaleValue);
			CapsuleShape.CallDeferred("set", "radius", val);
		}
	}
	public void _on_Projectile_body_entered(Node2D body){
		var terrain = body as TileMap;
		var hitable = body as IHitbox;

        if (hitable != null)
        {
			var hi = new HitInfo(this, Damage, Effect, Knockback, Hitstun);
			hitable.Hit(hi);
			if (explodes) _explode();
			else if (dissolves) Dissolve();
        }
				
		if (terrain != null){
			if (explodes) _explode();
			else if (dissolves) Dissolve();
		}
	}
	public virtual void ApplyCastInfo(CastInfo ci, ProjectileInfo pi)
    {
		Direction = ci.Direction.Normalized();
		Rotation = Direction.Angle();

		Damage = pi.Damage;
		Speed = pi.Speed;
		Knockback = pi.Knockback * Globals.CELL_SIZE;
		Hitstun = pi.HitstunTime;
		CollisionMask = pi.Hitmask;
		Position = pi.Position;
		ScaleValue = pi.ScaleValue;
		GD.Print(ScaleValue);
    }

	public override void _PhysicsProcess(float delta)
	{
		if (!Direction.IsNormalized()) {
			Direction = Direction.Normalized();
		}
		this.Position += Direction * Speed;
	}
	public void Reflect(uint new_hitmask, Vector2 new_direction, float speedMod, float damageMod)
	{
		Damage *= damageMod;
		Speed *= speedMod;
		ReflectsCount += 1;
		if (ReflectsCount >= MaxReflects || !Reflectable) {
			_explode();
			
		}else{
			this.CollisionMask = new_hitmask;
			Direction = Direction.Reflect(new_direction);
			this.Rotation = Direction.Angle();
		}
	}
	public virtual void _explode()
	{
		Dissolve();
	}

	public virtual void Dissolve()
	{
		GD.Print(this);
		if (DissolveTimer.IsStopped()){
			this.CollisionMask = 0;
			Speed = 12.5f;
			particles.Emitting = false;
			DissolveTimer.Start();
		}
	}

	public void _on_DissolveTimer_timeout()
	{
		QueueFree();
	}
}

