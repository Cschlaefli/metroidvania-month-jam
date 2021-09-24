using Godot;
using System;

public class Hurtbox : Area2D
{
	[Export]
	float Damage = 1;
	[Export]
	Damage.dam_types Types = global::Damage.dam_types.None;
	[Export]
	Vector2 Knockback = Vector2.Zero;
	[Export]
	float HitstunTime = .1f;
	
	[Signal]
    public delegate void Hit();

    public override void _Ready()
    {
		Connect("body_entered", this, "OnHit");
		Connect("area_entered", this, "OnHit");
        base._Ready();
    }
	public virtual void OnHit(Node body)
    {
		var hitbox = body as IHitbox;
		if(hitbox != null)
        {
			var hi = new HitInfo(this, Damage, Types, Knockback, HitstunTime);
			hitbox.Hit(hi);
			EmitSignal("Hit");
        }
    }
}
