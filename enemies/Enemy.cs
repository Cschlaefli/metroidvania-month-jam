using Godot;
using System;
using Stateless;
using Stateless.Graph;

public class Enemy : Node2D, IHitbox
{

    PackedScene Mana;
    PackedScene PackedEnemy;
    Vector2 Velocity = Vector2.Zero;
    [Export]
    float MaxHp { get; set; } = 0;
    [Export]
    float Gravity { get; set; } = 8;
    [Export] 
    float TerminalVelocity { get; set; } = 0;
    [Export]
    int ManaDropped { get; set; } = 1;
    [Export]
    double ManaValue { get; set; } = 5.0;
    [Export]
    float ShootRand { get; set; } = 1;
    [Export]
    float ShootInterval { get; set; } = 2;
    float CurrentHp;

    KinematicBody2D CurrentEnemy;
    Area2D Hurtbox;
    Timer FearTimer;
    Timer CastTimer;
    Timer RecoveryTimer;
    Timer ShootTimer;
    Timer FrozenTimer;
    Timer HitstunTimer;
    RayCast2D LineOfSight;

    double PlayerDistance = 1;
    Vector2 PlayerDirection = Vector2.Zero;
    bool CanSeePlayer = false;
    [Signal]
    delegate void OnDie();

    Spell CastingSpell;

    protected enum State { Disabled, Hitstun, Idle, Agro, Casting, Recovery, Dead }
    protected enum Trigger {  Hit, Die, Cast, Respawn, Recover, Wake, Agro, Sleep, Forget }
    protected enum StatusState {  None, Frozen, Fear}
    protected enum StatusTrigger { None, Freeze, Afraid }

    protected readonly StateMachine<State, Trigger> _sm = new StateMachine<State, Trigger>(State.Disabled);
    protected readonly StateMachine<StatusState, StatusTrigger> StatusStateMachine = new StateMachine<StatusState, StatusTrigger>(StatusState.None);
    protected StateMachine<State, Trigger>.TriggerWithParameters<HitInfo> HitTrigger;
    public override void _Ready()
    {
        CurrentEnemy = GetNode<KinematicBody2D>("CurrentEnemy");
        PackedEnemy = new PackedScene();
        foreach(Node child in CurrentEnemy.GetChildren())
        {
            foreach(Node ch in child.GetChildren())
            {
                ch.Owner = CurrentEnemy;
            }
            child.Owner = CurrentEnemy;
        }
        PackedEnemy.Pack(CurrentEnemy);
        CurrentEnemy.Connect("OnHit", this, "Hit");

        FearTimer = GetNode<Timer>("FearTimer");
        CastTimer = GetNode<Timer>("CastTimer");
        RecoveryTimer = GetNode<Timer>("RecoveryTimer");
        ShootTimer = GetNode<Timer>("ShootTimer");
        FrozenTimer = GetNode<Timer>("FrozenTimer");
        HitstunTimer = GetNode<Timer>("HitstunTimer");
        LineOfSight = GetNode<RayCast2D>("EnemyBody/LineOfSight");
        Hurtbox = GetNode<Area2D>("EnemyBody/Hurtbox");


        _sm.OnUnhandledTrigger((state, trigger) => {
            GD.Print($"Invalid trigger {trigger} in {state}");
            });
        HitTrigger = _sm.SetTriggerParameters<HitInfo>(Trigger.Hit);


        _sm.Configure(State.Disabled)
            .InternalTransition(Trigger.Respawn, t => _Respawn())
            .Permit(Trigger.Wake, State.Idle);

        _sm.Configure(State.Dead)
            .Permit(Trigger.Respawn, State.Idle)
            .Permit(Trigger.Sleep, State.Disabled);

        _sm.Configure(State.Hitstun)
            .OnEntryFrom(HitTrigger, hi => Hit(hi))
            .Permit(Trigger.Recover, State.Agro)
            .Permit(Trigger.Sleep, State.Disabled);

        _sm.Configure(State.Idle)
            .Permit(Trigger.Hit, State.Hitstun)
            .Permit(Trigger.Cast, State.Casting)
            .Permit(Trigger.Die, State.Dead)
            .Permit(Trigger.Agro, State.Agro)
            .Permit(Trigger.Sleep, State.Disabled);

        _sm.Configure(State.Casting)
            .OnEntry(() => StartCasting())
            .Permit(Trigger.Cast, State.Recovery)
            .PermitIf(Trigger.Hit, State.Hitstun, () => CanCastInterrupt())
            .Permit(Trigger.Die, State.Dead)
            .Permit(Trigger.Sleep, State.Disabled);

        _sm.Configure(State.Recovery)
            .OnEntryFrom( Trigger.Cast, () => Cast())
            .Permit(Trigger.Forget, State.Idle)
            .Permit(Trigger.Agro, State.Agro)
            .Permit(Trigger.Hit, State.Hitstun)
            .Permit(Trigger.Die, State.Dead)
            .Permit(Trigger.Sleep, State.Disabled);

//only debug
        WriteStateMachine();
    }

    public bool CanCastInterrupt() 
    {
        return CastingSpell?.Interruptable ?? true;
    }

    public virtual void OnCastTimerTimeout() => _sm.Fire(Trigger.Cast);
    public virtual void OnRecoveryTimerTimeout() => _sm.Fire(Trigger.Recover);

    public virtual void StartCasting()
    {
        CastingSpell.StartCasting();
        CastTimer.Start(CastingSpell.CastingTime);
    }

    public virtual void Cast()
    {
        var ci = new CastInfo() { By = this, Position = CurrentEnemy.Position, Direction = PlayerDirection };
        CastingSpell.Cast(ci);
        CastingSpell = null;
    }

    public virtual void _Respawn()
    {
        Velocity = Vector2.Zero;
        CurrentEnemy = PackedEnemy.Instance<KinematicBody2D>();
        AddChild(CurrentEnemy);
        LineOfSight = GetNode<RayCast2D>("EnemyBody/LineOfSight");
        Hurtbox = GetNode<Area2D>("EnemyBody/Hurtbox");
        CurrentEnemy.Connect("OnHit", this, "Hit");
        ShootInterval += (float)GD.RandRange(-ShootRand, ShootRand);
        CurrentHp = MaxHp;
        CurrentEnemy.Position = Vector2.Zero;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        _StateLogic(delta);
    }

    public virtual void _HandleGravity(float delta)
    {
        if (!CurrentEnemy.IsOnFloor())
        {
            Velocity.y += Gravity * delta * Globals.CELL_SIZE;
            Velocity.y = Math.Min(Velocity.y, TerminalVelocity * Globals.CELL_SIZE);
        }
    }
    public virtual void _ApplyMovement(float delta)
    {
        CurrentEnemy.MoveAndSlide(Velocity, Vector2.Up);
    }
    public virtual void _UpdatePlayerPosition()
    {
        var temp = Globals.PlayerPositon - this.GlobalPosition;
        PlayerDistance = temp.Length();
        PlayerDirection = temp.Normalized();
        LineOfSight.CastTo = temp;

        CanSeePlayer = !LineOfSight.IsColliding();
    }

    public virtual void _StateLogic(float delta)
    {
        switch (_sm.State)
        {
            case State.Recovery :
                if (RecoveryTimer.IsStopped())  _sm.Fire(Trigger.Agro);
                break;
            case State.Hitstun :
                if (HitstunTimer.IsStopped())  _sm.Fire(Trigger.Agro);
                break;
            default :
                break;
        }
    }

    public void Sleep() => _sm.Fire(Trigger.Sleep);
    public void Wake() => _sm.Fire(Trigger.Wake);
    public void ExitHitstun()
    {
            Hurtbox.CollisionLayer = 2;
            HitstunTimer.Stop();
            var c = Modulate;
            c.a = 1.0f;
            Modulate = c;
            if (CurrentEnemy != null)
                CurrentEnemy.CollisionLayer = 8;
    }
    public void Hit(HitInfo hi)
    {
        CurrentHp -= hi.Damage;
        if (CurrentHp > 0)
        {
            _sm.Fire(HitTrigger, hi.HitstunTime);
            Hurtbox.CollisionLayer = 0;
            CurrentEnemy.CollisionLayer = 0;
            var c = Modulate;
            c.a = 0.0f;
            Modulate = c;
        }
        else
        {
            _sm.Fire(Trigger.Die);
        }
    }

    public void WriteStateMachine()
    {
        string graph = UmlDotGraph.Format(_sm.GetInfo());
        GD.Print(graph);
        var f = new File();
        f.Open("user://EnemyGraph.json", File.ModeFlags.Write);
        f.StoreString(graph);
        f.Close();
    }
}
