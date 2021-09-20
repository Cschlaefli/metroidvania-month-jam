using Godot;
using System;

public interface IHitbox
{
	Node controller { get; }
	[Signal]

	delegate void OnHit(Node2D by, float damage, Damage.dam_types type, Vector2 knockback, float hitstun_time);
	void Hit(Node2D by, float damage, Damage.dam_types type, Vector2? knockback = null, float hitstun_time = .1f);
}
