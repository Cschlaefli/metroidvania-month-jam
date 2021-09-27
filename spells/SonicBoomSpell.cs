using Godot;
using System;

public class SonicBoomSpell : Spell
{

	PackedScene projectile;
	int lineDistance = 1000;
	Line2D guideLine;
	RayCast2D raycast2D;
	float guidelineAlpha = .75f;

    public override void _Ready()
    {
        base._Ready();
		projectile = GD.Load<PackedScene>("res://projectiles/SonicBoomProjectile.tscn");
		guideLine = GetNode<Line2D>("GuideLine");
    }
    public override void ShowGuide(float delta)
    {
        base.ShowGuide(delta);
        if (!Guide)
		{
			guideLine.Visible = false;
			return;
		}
		if(ChargeValue > 1)
        {
			CastingCost = BaseCost + (ChargeCost * ChargePercent);
			ProjectileDamage = BaseDamage + (ChargeDamage * ChargePercent);
			ProjectileSpeed = BaseSpeed + (ChargeSpeed * ChargePercent);
			Knockback = BaseKnockback * (ChargeSpeed * ChargePercent);
			Recoil = BaseRecoil + (ChargeRecoil * ChargePercent);
        }
        else
        {
			CastingCost = BaseCost;
			ProjectileDamage = BaseDamage;
			ProjectileSpeed = BaseSpeed;
			Knockback = BaseKnockback;

        }
		guideLine.Visible = true;
		var color = new Color(Colors.Black, guidelineAlpha);
		if (!CanCast) color = new Color(Colors.Red, guidelineAlpha);
		guideLine.ClearPoints();
		guideLine.AddPoint(Position);
		guideLine.AddPoint(Globals.Player.CastDirection * lineDistance + Position);
		guideLine.DefaultColor = color;
    }

    public override void Cast(CastInfo ci)
    {
        base.Cast(ci);
		var toAdd = projectile.Instance<Projectile>();
		toAdd.ApplyCastInfo(ci, projectileInfo);
		projectiles.AddChild(toAdd);
		ci.By.Velocity -= ci.Direction * Recoil;
    }
}
