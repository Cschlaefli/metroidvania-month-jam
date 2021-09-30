using Godot;
using System;

public class FlyingEnemy : Enemy
{
    [Export]
    float Speed = 3 * Globals.CELL_SIZE;
    [Export]
    float Acceleration = 14 * Globals.CELL_SIZE;
    [Export]
    float AgroRange = 7 * Globals.CELL_SIZE;
    [Export]
    float CastDist = 5.5f * Globals.CELL_SIZE;
    [Export]
    float BounceInterval = 10;

    Area2D BounceCheck;
    RayCast2D WallCheck;
    Timer BounceTimer;


    public Vector2 direction = Vector2.Zero;

    public override void _Ready()
    {
		base._Ready();
        BounceCheck = GetNode<Area2D>("EnemyBody/BounceCheck");
        WallCheck = GetNode<RayCast2D>("EnemyBody/WallCheck");
        DefaultSpell = GetNode<Spell>("EnemyBody/BasicSpell");

        direction = Vector2.Up.Rotated((float)GD.RandRange(Mathf.Pi, -Mathf.Pi));
        WallCheck.CastTo = direction * 1000;

        BounceTimer = GetNode<Timer>("BounceTimer");

    }
    protected override void _Respawn()
    {
        base._Respawn();
        BounceCheck = GetNode<Area2D>("EnemyBody/BounceCheck");
        WallCheck = GetNode<RayCast2D>("EnemyBody/WallCheck");
        DefaultSpell = GetNode<Spell>("EnemyBody/BasicSpell");
    }
    public void ChangeDirection()
    {

        if (_sm.State == State.Disabled || _sm.State == State.Dead) return;

        BounceTimer.Start(BounceInterval + (GD.Randf()*2) -1);

        if(WallCheck?.IsColliding() ?? false)
        {
            direction = direction.Bounce(WallCheck.GetCollisionNormal());
        }
        else
        {
            direction = Vector2.Up.Rotated(Helpers.RandAngle());
        }
        WallCheck.CastTo = 1000 * direction;
        Velocity = direction * 1 * Globals.CELL_SIZE;

        _sm.Fire(Trigger.SetRecovery);
        RecoveryTimer.Start(.25f);
    }

    protected override void _HandleState(State s, float delta)
    {
        base._HandleState(s, delta);
        switch (_sm.State)
        {
            case State.Idle:
                if (ShouldAgro())
                {
                    _sm.Fire(Trigger.Agro);
                }
                else if (BounceCheck.GetOverlappingAreas().Count != 0 || BounceCheck.GetOverlappingBodies().Count != 0)
                {
                    ChangeDirection();
                }
                else 
                { 
                    Velocity = Helpers.Accelerate(Velocity, direction * Speed, Acceleration, delta);
                }
                break;
            case State.Agro:
                if (CanSeePlayer)
                {
                    direction = PlayerDirection;
                    WallCheck.CastTo = direction;
                }

                if(PlayerDistance <= CastDist + Globals.CELL_SIZE)
                {
                    Velocity = Helpers.Accelerate(Velocity, -direction * Speed, Acceleration, delta);
                }
                else if(CanSeePlayer)
                {
                    Velocity = Helpers.Accelerate(Velocity, direction * Speed, Acceleration, delta);
                }
                break;
            case State.Recovery :
                Velocity = Helpers.Accelerate(Velocity, Velocity.Normalized() * 200, Acceleration, delta);
                break;
            default:
                break;
        }
    }
    public override bool CanShoot()
    {
        return CanSeePlayer && PlayerDistance <= CastDist * 2;
    }
    protected override bool ShouldAgro()
    {
        return (CanSeePlayer && PlayerDistance <= AgroRange);
    }
    public override void OnHurtboxHit()
    {
        base.OnHurtboxHit();
        _sm.Fire(Trigger.SetRecovery);
        RecoveryTimer.Start(.5f);
    }

}
