using Godot;
using System;

public class ReflectorEnemy : Enemy
{
    Reflector reflector;
    public override void _Ready()
    {
        base._Ready();
        reflector = GetNode<Reflector>("EnemyBody/Reflector");
        reflector.Activate();
    }

    protected override void _Respawn()
    {
        base._Respawn();
        reflector = GetNode<Reflector>("EnemyBody/Reflector");
        reflector.Activate();
    }
}
