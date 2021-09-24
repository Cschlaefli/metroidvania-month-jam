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
        base._HandleState(s, delta);
        switch (s)
        {
			case State.Idle :
			case State.Agro :
				Velocity = Velocity.LinearInterpolate(new Vector2() { x= 0, y= Velocity.y }, delta * 5);
                if (ShootTimer.IsStopped())
                {
					_sm.Fire(Trigger.Cast);
					ShootTimer.Start(ShootInterval);
                }
				break;
			default:
				break;
        }
    }
    public override void StartCasting()
    {
		CastingSpell = GetNode<Spell>("EnemyBody/Hop");
        base.StartCasting();
    }
    protected override void OnHurtboxHit()
    {
		Velocity = -Velocity;
    }

}
