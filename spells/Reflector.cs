using Godot;
using System;

public class Reflector : Area2D
{
	[Export(PropertyHint.Flags)]
	public uint ReflectHitmask = 9;
	[Export(PropertyHint.Flags)]
	public uint ReflectMask = 4;

	Vector2 Direction = Vector2.Zero;

	[Export]
	float SpeedMod = 1.5f;
	[Export]
	float DamageMod = 1.2f;
	[Export]
	bool DirectReflect = false;

	[Signal]
	delegate void Reflected(Projectile body);
    public override void _Ready()
    {
        base._Ready();
		Connect("area_entered", this, nameof(OnReflectorEntered));
		Connect("body_entered", this, nameof(OnReflectorEntered));
    }
	public void Activate() => CollisionMask = ReflectMask;
	public void Deactivate() => CollisionMask = 0;
	void OnReflectorEntered(IReflectable reflectable)
    {
		if (reflectable != null)
        {
			EmitSignal(nameof(Reflected), reflectable);
			if (DirectReflect)
			{
				reflectable.Reflect(ReflectHitmask, reflectable.Direction.Rotated((float)Math.PI * .5f), SpeedMod, DamageMod);
			}
            else
            {
				reflectable.Reflect(ReflectHitmask, Vector2.Up.Rotated((float)(Math.PI * .5)), SpeedMod, DamageMod);
            }
        }
    }
}
