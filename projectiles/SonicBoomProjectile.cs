using Godot;
using System;

public class SonicBoomProjectile : Projectile, IExplodes
{
	PackedScene Boom;
	
	public override void _Ready()
    {
		base._Ready();
		Boom =	GD.Load<PackedScene>($"res://projectiles/SonicBoomBurst.tscn");
    }

	public override void _explode()
	{
		base.Dissolve();
		var add = Boom.Instance<Projectile>();
		add.GlobalPosition = GlobalPosition;
		GetParent().AddChild(add);
	}
}
