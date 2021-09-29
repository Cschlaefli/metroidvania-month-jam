using Godot;
using System;
using Stateless;
using Stateless.Graph;

public class Enemy : Node2D, IHitbox, ICaster
{

    PackedScene Mana;
    PackedScene PackedEnemy;
    EnemySpawner Spawner;
    public uint SpellHitmask { get => 3; }
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
    protected float ShootInterval { get; set; } = 4;
    [Export]
    bool ShootWhileIdle = false;
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

    protected float PlayerDistance = float.MaxValue;
    protected Vector2 PlayerDirection = Vector2.Zero;
    public bool CanSeePlayer => !LineOfSight?.IsColliding() ?? false;
    [Signal]
    delegate void Die();

    public Spell CastingSpell;
    public Spell DefaultSpell;

    protected enum State { Disabled, Hitstun, Idle, Agro, Casting, Recovery, Dead }
    protected enum Trigger { Hit, Die, Cast, Respawn, Recover, Wake, Agro, Sleep, Forget, Shoot, SetRecovery }
    protected enum StatusState { None, Frozen, Fear }
    protected enum StatusTrigger { None, Freeze, Afraid }

    protected readonly StateMachine<State, Trigger> _sm = new StateMachine<State, Trigger>(State.Disabled);
    protected readonly StateMachine<StatusState, StatusTrigger> StatusStateMachine = new StateMachine<StatusState, StatusTrigger>(StatusState.None);
    public override void _Ready()
    {
        Spawner = GetParent<EnemySpawner>();
        CurrentEnemy = GetNode<EnemyBody>("EnemyBody");
        PackedEnemy = new PackedScene();
        foreach (Node child in CurrentEnemy.GetChildren())
        {
            foreach (Node ch in child.GetChildren())
            {
                ch.Owner = CurrentEnemy;
            }
            child.Owner = CurrentEnemy;
        }
        PackedEnemy.Pack(CurrentEnemy);
        CurrentEnemy.Connect(nameof(EnemyBody.OnHit), this, nameof(Hit));
        Mana = GD.Load<PackedScene>("res://enemies/ManaPellet.tscn");

        FearTimer = GetNode<Timer>("FearTimer");
        CastTimer = GetNode<Timer>("CastTimer");
        RecoveryTimer = GetNode<Timer>("RecoveryTimer");
        ShootTimer = GetNode<Timer>("ShootTimer");

        FrozenTimer = GetNode<Timer>("FrozenTimer");
        HitstunTimer = GetNode<Timer>("HitstunTimer");
        LineOfSight = GetNode<RayCast2D>("EnemyBody/LineOfSight");
        Hurtbox = GetNode<Hurtbox>("EnemyBody/Hurtbox");
        CurrentHp = MaxHp;



        _sm.OnUnhandledTrigger((state, trigger) => {
            //if(state != State.Disabled && state != State.Dead)
                //GD.Print($"Invalid trigger {trigger} in {state}");
        });


        _sm.Configure(State.Disabled)
            .OnEntry(() => NoHit())
            .OnExit(() => { ExitHitstun(); ShootTimer.Start(ShootInterval);})
            .InternalTransition(Trigger.Respawn, () => _Respawn())
            .PermitIf(Trigger.Wake, State.Idle, () => !ShouldAgro())
            .PermitIf(Trigger.Wake, State.Agro, () => ShouldAgro());

        _sm.Configure(State.Dead)
            .OnEntry(() => OnDie())
            .Permit(Trigger.Respawn, State.Idle)
            .Permit(Trigger.Sleep, State.Disabled);

        _sm.Configure(State.Hitstun)
            .OnExit(() => ExitHitstun())
            .PermitIf(Trigger.Recover, State.Agro, () => ShouldAgro())
            .PermitIf(Trigger.Recover, State.Idle, () => !ShouldAgro())
            .Permit(Trigger.Sleep, State.Disabled);

        _sm.Configure(State.Idle)
            .OnEntry(() => { if (ShootWhileIdle) RestartShootTimer(); })
            .Permit(Trigger.SetRecovery, State.Recovery)
            .Permit(Trigger.Hit, State.Hitstun)
            .PermitIf(Trigger.Shoot, State.Casting, () => ShootWhileIdle && CanShoot())
            .Permit(Trigger.Die, State.Dead)
            .Permit(Trigger.Agro, State.Agro)
            .Permit(Trigger.Sleep, State.Disabled);

        _sm.Configure(State.Casting)
            .OnEntry(() => StartCasting())
            .Permit(Trigger.Cast, State.Recovery)
            .PermitIf(Trigger.Hit, State.Hitstun, () => CanCastInterrupt())
            .Permit(Trigger.Die, State.Dead)
            .Permit(Trigger.Sleep, State.Disabled);

        //add bool function to check if should be agro
        _sm.Configure(State.Recovery)
            .OnEntryFrom(Trigger.Cast, () => Cast())
            .PermitIf(Trigger.Recover, State.Idle, () => !ShouldAgro())
            .PermitIf(Trigger.Recover, State.Agro, () => ShouldAgro())
            .Permit(Trigger.Agro, State.Agro)
            .Permit(Trigger.Hit, State.Hitstun)
            .Permit(Trigger.Die, State.Dead)
            .Permit(Trigger.Sleep, State.Disabled);

        _sm.Configure(State.Agro)
            .OnEntry(() => RestartShootTimer())
            .Permit(Trigger.SetRecovery, State.Recovery)
            .Permit(Trigger.Forget, State.Idle)
            .PermitIf(Trigger.Shoot, State.Casting, () => CanShoot())
            .Permit(Trigger.Hit, State.Hitstun)
            .Permit(Trigger.Die, State.Dead)
            .Permit(Trigger.Sleep, State.Disabled);

        //only debug
        WriteStateMachine();
    }

    protected virtual bool ShouldAgro()
    {
        return true;
    }
    public virtual bool CanShoot()
    {
        return true;
    }

    public bool CanCastInterrupt()
    {
        return CastingSpell?.Interruptable ?? true;
    }

    public virtual void OnCastTimerTimeout() => _sm.Fire(Trigger.Cast);
    public virtual void OnRecoveryTimerTimeout() => _sm.Fire(Trigger.Recover);
    public virtual void OnShootTimerTimeout()
    {
        _sm.Fire(Trigger.Shoot);
    }
    public virtual void RestartShootTimer() => ShootTimer.Start(ShootInterval + ((GD.Randf() - .5f) * ShootRand));
    public virtual void OnHitstunTimerTimeout(){
        _sm.Fire(Trigger.Recover);
    }

    public virtual void StartCasting()
    {
        if (DefaultSpell == null) return;
        CastingSpell ??= DefaultSpell;
        var ci = new CastInfo() { By = this, Position = CurrentEnemy.Position, Direction = PlayerDirection };
        CastingSpell.StartCasting(ci);
        CastTimer.Start(CastingSpell.CastingTime);
    }

    public virtual void Cast()
    {
        if (DefaultSpell == null) return;
        CastingSpell ??= DefaultSpell;
        var ci = new CastInfo() { By = this, Position = CurrentEnemy.Position, Direction = PlayerDirection };
        CastingSpell.Cast(ci);
        RecoveryTimer.Start(CastingSpell.RecoveryTime);
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
        EmitSignal(nameof(Die));

    }

    protected virtual void _Respawn()
    {
        if(CurrentEnemy != null)
        {
            return;
        }
        Velocity = Vector2.Zero;
        CurrentEnemy = PackedEnemy.Instance<EnemyBody>();
        AddChild(CurrentEnemy);
        LineOfSight = GetNode<RayCast2D>("EnemyBody/LineOfSight");
        Hurtbox = GetNode<Hurtbox>("EnemyBody/Hurtbox");

        Hurtbox.Connect(nameof(Hurtbox.Hit), this, nameof(OnHurtboxHit));

        CurrentEnemy.Connect(nameof(EnemyBody.OnHit), this, nameof(Hit));

        ShootInterval += (float)GD.RandRange(-ShootRand, ShootRand);
        CurrentHp = MaxHp;
        CurrentEnemy.Position = Vector2.Zero;
    }
    public virtual void OnHurtboxHit() => Velocity = -Velocity;

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
            Velocity = new Vector2(Velocity.x, y);
        }
    }
    protected virtual void _ApplyMovement(float delta)
    {
        if (CurrentEnemy == null) return;
        
        var pos = CurrentEnemy.GlobalPosition + (Velocity * delta * 6);
        if (Spawner.ContainingScreen.screenLimits.IsOutsideLimits(pos) )
        {
            Velocity = -Velocity;
        }

        CurrentEnemy?.MoveAndSlide(Velocity, Vector2.Up);
    }
    protected virtual void _UpdatePlayerPosition()
    {
        var temp = Globals.Player.GlobalPosition - CurrentEnemy.GlobalPosition;
        PlayerDistance = temp.Length();
        PlayerDirection = temp.Normalized();
        LineOfSight.CastTo = temp;

    }

    protected virtual void _StateLogic(float delta)
    {

        if (HasNode("EnemyBody"))
        {
            var n = GetNode<Label>("EnemyBody/StateLabel");
            if (n != null) n.Text = _sm.State.ToString();
        }
        _HandleState(_sm.State, delta);
        if(_sm.State != State.Disabled && _sm.State != State.Dead)
        {
            _ApplyMovement(delta);
            _UpdatePlayerPosition();
        }

        switch (_sm.State)
        {
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

    public void NoHit()
    {
        if (CurrentEnemy != null)
        {
            CurrentEnemy.CollisionLayer = 0;
            Hurtbox.CollisionLayer = 0;
        }
    }

    public void ExitHitstun()
    {
        HitstunTimer.Stop();
        var c = Modulate;
        c.a = 1.0f;
        Modulate = c;
        if (CurrentEnemy != null)
        {
            CurrentEnemy.CollisionLayer = 8;
            Hurtbox.CollisionLayer = 2;
        }
    }
    public void Hit(HitInfo hi)
    {
        CurrentHp -= hi.Damage;
        if (CurrentHp > 0)
        {
            _sm.Fire(Trigger.Hit);
            HitstunTimer.Start(hi.HitstunTime);
            NoHit();
            var c = Modulate;
            c.a = 0.3f;
            Modulate = c;
        }
        else
        {
            _sm.Fire(Trigger.Die);
        }
    }

    public void WriteStateMachine()
    {
        /*
        string graph = UmlDotGraph.Format(_sm.GetInfo());
        GD.Print(graph);
        var f = new File();
        f.Open("user://EnemyGraph.json", File.ModeFlags.Write);
        f.StoreString(graph);
        f.Close();
        */
    }
}
