using Godot;
using System;

public class Hitbox : Node2D, IHitbox
{
    public Node controller { get => GetParent(); }

    public void Hit(HitInfo hi)
    {
        EmitSignal("OnHit", hi);
    }
}
