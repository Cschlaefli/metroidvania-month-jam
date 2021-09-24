using Godot;
using System;
using Stateless;
using Stateless.Graph;

public class Enemy : Node2D, IHitbox, ICaster
{

    PackedScene Mana;
    PackedScene PackedEnemy;
    public Vector2 Velocity { get; set; } = Vector2.Zero;
    [Export]
    float MaxHp { get; set; } = 0;
    [Export]
    public float Gravity { get; set; } = 8;
    [Export] 
    float TerminalVelocity { get; set; } = 5;
    [Export]
    int ManaDropped { get; set; } = 1;
    [Export]
    float ManaValue { get; set; } = 5.0f;
    [Export]
    float ShootRand { get; set; } = 1;
    [Export]
    protected float ShootInterval { get; set; } = 2;
    float CurrentHp;

    EnemyBody CurrentEnemy;
    Hurtbox Hurtbox;
    protected Timer FearTimer;
    protected Timer CastTimer;
    protected Timer RecoveryTimer;
    protected Timer ShootTimer;
    protected Timer FrozenTimer;
    protected Timer HitstunTimer;
    RayCast2D LineOfSight;

    double PlayerDistance = 1;
    Vector2 PlayerDirection = Vector2.Zero;
    bool CanSeePlayer = false;
    [Signal]
    delegate void Die();

    public Spell CastingSpell;

    protected enum State { Disabled, Hitstun, Idle, Agro, Casting, Recovery, Dead }
    protected enum Trigger {  Hit, Die, Cast, Respawn, Recover, Wake, Agro, Sleep, Forget, Shoot }
    protected enum StatusState {  None, Frozen, Fear}
    protected enum StatusTrigger { None, Freeze, Afraid }

    protected readonly StateMachine<State, Trigger> _sm = new StateMachine<State, Trigger>(State.Disabled);
    protected readonly StateMachine<StatusState, StatusTrigger> StatusStateMachine = new StateMachine<StatusState, StatusTrigger>(StatusState.None);
    protected StateMachine<State, Trigger>.TriggerWithParameters<float> HitTrigger;
    public override void _Ready()
    {
        CurrentEnemy = GetNode<EnemyBody>("EnemyBody");
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
        Mana = GD.Load<PackedScene>("res://enemies/ManaPellet.tscn");

        FearTimer = GetNode<Timer>("FearTimer");
        CastTimer = GetNode<Timer>("CastTimer");
        RecoveryTimer = GetNode<Timer>("RecoveryTimer");
        ShootTimer = GetNode<Timer>("ShootTimer");

        FrozenTimer = GetNode<Timer>("FrozenTimer");
        HitstunTimer = GetNode<Timer>("HitstunTimer");
        LineOfSight = GetNode<RayCast2D>("EnemyBody/LineOfSight");
        Hurtbox = GetNode<Hurtbox>("EnemyBody/Hurtbox");



        _sm.OnUnhandledTrigger((state, trigger) => {
            GD.Print($"Invalid trigger {trigger} in {state}");
            });
        HitTrigger = _sm.SetTriggerParameters<float>(Trigger.Hit);


        _sm.Configure(State.Disabled)
            .InternalTransition(Trigger.Respawn, t => _Respawn())
            .Permit(Trigger.Wake, State.Idle);

        _sm.Configure(State.Dead)
            .OnEntry(() => OnDie())
            .Permit(Trigger.Respawn, State.Idle)
            .Permit(Trigger.Sleep, State.Disabled);

        _sm.Configure(State.Hitstun)
            .OnEntryFrom(HitTrigger, t => HitstunTimer.Start(t))
            .Permit(Trigger.Recover, State.Agro)
            .Permit(Trigger.Sleep, State.Disabled);

        _sm.Configure(State.Idle)
            .Permit(Trigger.Hit, State.Hitstun)
            .Permit(Trigger.Shoot, State.Casting)
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

        _sm.Configure(State.Agro)
            .Permit(Trigger.Shoot, State.Casting)
            .Permit(Trigger.Forget, State.Idle)
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
    public virtual void OnShootTimerTimeout() => _sm.Fire(Trigger.Shoot);

    public virtual void StartCasting()
    {
        var ci = new CastInfo() { By = this, Position = CurrentEnemy.Position, Direction = PlayerDirection };
        CastingSpell.StartCasting(ci);
        CastTimer.Start(CastingSpell.CastingTime);
    }

    public virtual void Cast()
    {
        var ci = new CastInfo() { By = this, Position = CurrentEnemy.Position, Direction = PlayerDirection };
        CastingSpell.Cast(ci);
        CastingSpell = null;
    }

    protected virtual void OnDie() 
    {
        foreach(int i in GD.Range(ManaDropped))
        {
            var toAdd = Mana.Instance<ManaPellet>();
            toAdd.Amount = ManaValue;
            toAdd.Position = CurrentEnemy.Position;
            toAdd.Velocity = (Vector2.Up * Globals.CELL_SIZE * 3).Rotated((float)GD.RandRange(0, Math.PI * 2));
            CallDeferred("add_child", toAdd);
        }
        CurrentEnemy.QueueFree();
        CurrentEnemy = null;
        EmitSignal("OnDie");
    }

    protected virtual void _Respawn()
    {
        Velocity = Vector2.Zero;
        CurrentEnemy = PackedEnemy.Instance<EnemyBody>();
        AddChild(CurrentEnemy);
        LineOfSight = GetNode<RayCast2D>("EnemyBody/LineOfSight");
        Hurtbox = GetNode<Hurtbox>("EnemyBody/Hurtbox");

        Hurtbox.Connect(nameof(Hurtbox.OnHit), this, nameof(OnHurtboxHit));

        CurrentEnemy.Connect("OnHit", this, "Hit");

        ShootInterval += (float)GD.RandRange(-ShootRand, ShootRand);
        CurrentHp = MaxHp;
        CurrentEnemy.Position = Vector2.Zero;
    }
    protected virtual void OnHurtboxHit()
    {

    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        _StateLogic(delta);
    }

   protected virtual void _HandleGravity(float delta)
    {
        if (!CurrentEnemy.IsOnFloor())
        {
            var y = Math.Min(Velocity.y + Gravity * delta * Globals.CELL_SIZE, TerminalVelocity * Globals.CELL_SIZE);
            Velocity = new Vector2(Velocity.x, Velocity.y);
        }
    }
    protected virtual void _ApplyMovement(float delta)
    {
        CurrentEnemy.MoveAndSlide(Velocity, Vector2.Up);
    }
    protected virtual void _UpdatePlayerPosition()
    {
        var temp = Globals.PlayerPositon - this.GlobalPosition;
        PlayerDistance = temp.Length();
        PlayerDirection = temp.Normalized();
        LineOfSight.CastTo = temp;

        CanSeePlayer = !LineOfSight.IsColliding();
    }

    protected virtual void _StateLogic(float delta)
    {
        _HandleState(_sm.State, delta);

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
    protected virtual void _HandleState(State s, float delta)
    {

    }

    public void Respawn() => _sm.Fire(Trigger.Respawn);
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
