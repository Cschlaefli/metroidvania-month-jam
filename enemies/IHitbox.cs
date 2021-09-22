using Godot;
using System;

public class HitInfo : Godot.Object
{
	public HitInfo(Node2D by, float damage, Damage.dam_types type, Vector2? knockback = null, float hitstunTime = .1f)
    {
		By = by;
		Damage = damage;
		Type = type;
		Knockback = knockback ?? Vector2.Zero;
		HitstunTime = hitstunTime;
    }
	public Node2D By { get; set; }
	public 	float Damage { get; set; }
	public Damage.dam_types Type { get; set; }
	public Vector2 Knockback { get; set; } = Vector2.Zero;
	public float HitstunTime { get; set; } = .1f;

}

public interface IHitbox
{
	[Signal]

	delegate void OnHit(HitInfo hi);
	void Hit(HitInfo hi);
}
