using Godot;
using System;

public class HitInfo
{
	public HitInfo(Node2D by, double damage, Damage.dam_types type, Vector2? knockback = null, double hitstunTime = .1)
    {
		By = by;
		Damage = damage;
		Type = type;
		Knockback = knockback ?? Vector2.Zero;
		HitstunTime = hitstunTime;
    }
	Node2D By { get; set; }
	double Damage { get; set; }
	Damage.dam_types Type { get; set; }
	Vector2 Knockback { get; set; } = Vector2.Zero;
	double HitstunTime { get; set; } = .1;

}

public interface IHitbox
{
	[Signal]

	delegate void OnHit(HitInfo hi);
	void Hit(HitInfo hi);
}
