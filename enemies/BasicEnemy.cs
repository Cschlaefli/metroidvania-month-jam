using Godot;
using System;

public class BasicEnemy : Enemy
{
	Timer MovingTimer;
	Timer StandingTimer;
	[Export]
	public float speed = 2.2f;

    public override void _Ready()
    {
		MovingTimer = GetNode<Timer>("MovingTimer");
		StandingTimer = GetNode<Timer>("StandingTimer");
        base._Ready();
    }

    protected override void _HandleState(State s, float delta)
    {
        switch (s)
        {
			case State.Idle :
                _sm.Fire(Trigger.Agro);
                Velocity = Helpers.Accelerate(Velocity, Vector2.Zero, 1500, delta);
                _HandleGravity(delta);
                break;
			case State.Agro :
                Velocity = Helpers.Accelerate(Velocity, Vector2.Zero, 1500, delta);
                if (ShootTimer.IsStopped())
                {
					_sm.Fire(Trigger.Cast);
					ShootTimer.Start(ShootInterval);
                }
                _HandleGravity(delta);
				break;
            case State.Disabled:
            case State.Dead:
                break;
			default:
                _HandleGravity(delta);
				break;
        }
        base._HandleState(s, delta);
    }
    public override void StartCasting()
    {
		CastingSpell = GetNode<Spell>("EnemyBody/Hop");
        base.StartCasting();
    }
    public override void OnHurtboxHit()
    {
		Velocity = -Velocity;
    }

}
