using Godot;
using System;

public class EnemyBody : KinematicBody2D, IHitbox
{

	[Signal]
	public delegate void OnHit(HitInfo hi);

    public Node controller { get => GetParent(); }

    public void Hit(HitInfo hi)
    {
        EmitSignal("OnHit", hi);
    }

}
