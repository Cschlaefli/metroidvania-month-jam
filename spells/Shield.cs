using Godot;
using System;

public class Shield : Spell
{
    [Export]
    public float ReflectBonus = .1f;
    [Signal]
    public delegate void Reflected(Projectile reflectable);
    Reflector reflector;
    CPUParticles shieldParticles;
    public override void _Ready()
    {
        base._Ready();
        reflector = GetNode<Reflector>("Reflector");
        shieldParticles = GetNode<CPUParticles>("Reflector/ShieldParticles");
        ActiveTimer.Connect("timeout", this, nameof(OnActiveTimerTimeout));
    }
    public void OnActiveTimerTimeout()
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
