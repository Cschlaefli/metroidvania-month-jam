using Godot;
using System;

public class SonicBoomProjectile : Projectile, IExplodes
{
	PackedScene Boom;
	
	public override void _Ready()
    {	
		Boom =	GD.Load<PackedScene>($"res://projectiles/SonicBoomBurst.tscn");
    }

	public override void _dissolve()
    {
		_explode();
		base._dissolve();
    }

	public override void _explode()
	{
		base._dissolve();
		var add = Boom.Instance<Projectile>();
		add.GlobalPosition = GlobalPosition;
		this.AddChild(add);
	}
}
