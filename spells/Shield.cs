using Godot;
using System;

public class Shield : Spell
{
    [Export]
    public float ReflectBonus = .1f;
    [Signal]
    public delegate void Reflected(Projectile reflectable);
    Reflector reflector;
    Particles2D shieldParticles;
    public override void _Ready()
    {
        base._Ready();
        reflector = GetNode<Reflector>("Reflector");
        reflector.Connect(nameof(Reflector.Reflected), this, nameof(OnReflectorReflected));
        shieldParticles = GetNode<Particles2D>("ShieldParticles");
    }
    public override void OnActiveTimerTimeout()
    {
        reflector.Deactivate();
        shieldParticles.Emitting = false;
    }

    public override void Cast(CastInfo ci)
    {
        Rotation = ci.Direction.Angle();
        reflector.Activate();
        shieldParticles.Emitting = true;

        base.Cast(ci);
    }

    public void OnReflectorReflected(IReflectable reflectable)
    {
        if (!ActiveTimer.IsStopped()) {
            EmitSignal("reflected", reflectable);
            ActiveTimer.Start(ActiveTimer.TimeLeft + ReflectBonus);
        }
    }

}
