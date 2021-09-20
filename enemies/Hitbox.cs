using Godot;
using System;

public class Hitbox : Node2D, IHitbox
{
    public Node controller { get => GetParent(); }

    public void Hit(Node2D by, float damage, Damage.dam_types type, Vector2? knockback = null, float hitstun_time = .1f)
    {
        EmitSignal("OnHit", by, damage, type, knockback ?? Vector2.Zero, hitstun_time);
    }
}
