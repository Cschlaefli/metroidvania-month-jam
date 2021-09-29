using Godot;
using System;
using Stateless;
using Godot.Collections;

public class OrbBoss : Node2D, IPersist, IHitbox, ICaster
{

    public uint SpellHitmask { get => 3; }
	[Export]
	float Health = 48;
    [Export]
    float PhaseTwoThreshold = 40;
    [Export]
    float PhaseThreeThreshold = 20;

    float CurrentHp;
    float TeleportDelay = 2;

    float time = 3.14f;

    int CurrentTeleport = 0;

    Node2D Body;


    SpellBarrage spellBarrageOne;
    SpellBarrage spellBarrageTwo;
    Timer DeathTimer;
    Timer TeleportTimer;
    Sprite sprite;
    ShaderMaterial spriteMaterial;
    Array<Node2D> TeleportPoints = new Array<Node2D>();
    Node2D TeleSpawn;
    Node2D Telepoints;


    public override void _Ready()
    {
        base._Ready();
        Velocity = Vector2.Zero;
        TeleSpawn = GetNode<Node2D>("TeleSpawn");
        Telepoints = GetNode<Node2D>("Telepoints");
        spellBarrageOne = GetNode<SpellBarrage>("Body/PhaseOneShots");
        spellBarrageTwo = GetNode<SpellBarrage>("Body/PhaseTwoShots");


        var hb = GetNode<Hitbox>("Body/Hitbox");
        hb.Connect(nameof(Hitbox.OnHit), this, nameof(Hit));

        foreach(var child in Telepoints.GetChildren())
        {
            var point = child as Node2D;
            TeleportPoints.Add(point);
        }
        TeleportPoints.Add(Telepoints);
        TeleportPoints.Shuffle();

        Body = GetNode<Node2D>("Body");

        sprite = GetNode<Sprite>("Body/Sprite");
        spriteMaterial = sprite.Material as ShaderMaterial;


        DeathTimer = GetNode<Timer>("DeathTimer");
        DeathTimer.Connect("timeout", this, nameof(DyingTimeout));

        TeleportTimer = GetNode<Timer>("TeleportTimer");
        TeleportTimer.Connect("timeout", this, nameof(EndTeleport));

        CurrentHp = Health;
        if (Persist)
        {
            _sm = new StateMachine<State, Trigger>(State.Disabled);
        }
        else
        {
            _sm = new StateMachine<State, Trigger>(State.Dead);
        }

        ConfigureStateMachine();
    }


    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        time += delta;
        switch (_sm.State)
        {
            case State.Dying:
                HandleDying(delta);
                break;
            case State.Teleporting:
                HandleTeleporting(delta);
                break;
            case State.PhaseOne :
                sprite.Rotation = (Mathf.Pi / 8) * time;
                var x = Body.Position.x;
                var y = Body.Position.y;
                x -= Mathf.Sin(time * Mathf.Pi / 8) * 10;
                y += Mathf.Cos(time) * 15;
                Body.Position = new Vector2(x, y);
                break;
            case State.PhaseTwo :
                sprite.Rotation = (Mathf.Pi / 4) * time;
                if(time >= 5)
                {
                    time = 0;
                    _sm.Fire(Trigger.Teleport);
                    spellBarrageTwo.FireAll();
                }
                break;
            case State.PhaseThree :
                sprite.Rotation = (Mathf.Pi / 2) * time;
                if(time >= 5)
                {
                    time = 0;
                    _sm.Fire(Trigger.Teleport);
                    spellBarrageTwo.FireAll();
                }
                break;
            default:
                break;
        }
    }

    public virtual void HandleDying(float delta)
    {
        var time = DeathTimer.TimeLeft;

        spriteMaterial.SetShaderParam("amount", Mathf.Sin(time * 5) * 2);
        sprite.Modulate = new Color(sprite.Modulate, time * .1f);
    }
    public virtual void HandleTeleporting(float delta)
    {
        spriteMaterial.SetShaderParam("amount", Mathf.Sin(time * 20) * 2.5f);
    }

    protected enum State { Disabled, PhaseOne, PhaseTwo, PhaseThree, Teleporting, Dying, Dead }
    protected enum Trigger { NextPhase, Die, Cast, Teleport, Wake, Hit, EndTeleport, EndDying }
    public virtual void ConfigureStateMachine()
    {

        _sm.OnUnhandledTrigger((state, trigger) => {
           // if(state != State.Disabled && state != State.Dead)
                //GD.Print($"Invalid trigger {trigger} in {state}");
        });

        _sm.Configure(State.Disabled)
            .Permit(Trigger.Wake, State.PhaseOne);

        _sm.Configure(State.PhaseOne)
            .OnEntry(() => StartPhase(State.PhaseOne))
            .PermitIf(Trigger.NextPhase, State.PhaseTwo, () => CurrentHp <= PhaseTwoThreshold);

        _sm.Configure(State.PhaseTwo)
            .OnEntry(() => StartPhase(State.PhaseTwo))
            .OnExit(() => ExitPhase(State.PhaseTwo))
            .Permit(Trigger.Teleport, State.Teleporting)
            .PermitIf(Trigger.NextPhase, State.PhaseThree, () => CurrentHp <= PhaseThreeThreshold);

        _sm.Configure(State.PhaseThree)
            .OnEntry(() => StartPhase(State.PhaseThree))
            .OnExit(() => ExitPhase(State.PhaseThree))
            .Permit(Trigger.Teleport, State.Teleporting)
            .Permit(Trigger.Die, State.Dying);


        _sm.Configure(State.Teleporting)
            .OnEntry(() => TeleportTimer.Start(TeleportDelay))
            .PermitIf(Trigger.EndTeleport, State.PhaseTwo, () => CurrentHp <= PhaseTwoThreshold && CurrentHp >= PhaseThreeThreshold)
            .PermitIf(Trigger.EndTeleport, State.PhaseThree, () => CurrentHp <= PhaseThreeThreshold)
            .Permit(Trigger.Die, State.Dying);

        _sm.Configure(State.Dying)
            .Permit(Trigger.EndDying, State.Dead)
            .OnEntry(() => Die());

        _sm.Configure(State.Dead);
    }

    void DyingTimeout()
    {
        var tele = GD.Load<PackedScene>("res://screens/pickups/TeleportPickup.tscn");
        TeleSpawn.AddChild(tele.Instance());
        Body.Modulate = new Color(Body.Modulate, 0);
        _sm.Fire(Trigger.EndDying);
    }
    void EndTeleport()
    {
        spriteMaterial.SetShaderParam("amount", 0);
        if (CurrentTeleport >= TeleportPoints.Count) CurrentTeleport = 0;
        var newPos = TeleportPoints[CurrentTeleport].Position;
        CurrentTeleport += 1;
        Body.Position = newPos;
        _sm.Fire(Trigger.EndTeleport);
    }

    void Die()
    {
        spellBarrageOne.Deactivate();
        spellBarrageTwo.Deactivate();
        var hb = GetNode<Hitbox>("Body/Hitbox");
        hb.CollisionLayer = 0;
        var df = GetNode<Particles2D>("Body/Sprite/DeathEffect");
        df.Emitting = true;
        DeathTimer.Start(10);
        Persist = false;
    }

    protected virtual void ExitPhase(State st)
    {
        switch (st)
        {
            case State.PhaseOne :
                spellBarrageOne.Deactivate();
                break;
            case State.PhaseTwo :
                break;
            case State.PhaseThree :
                break;
        }
    }
    protected virtual void StartPhase(State st)
    {
        switch (st)
        {
            case State.PhaseOne :
                spellBarrageOne.Activate();
                break;
            case State.PhaseTwo :
                break;
            case State.PhaseThree :
                _sm.Fire(Trigger.Teleport);
                spellBarrageOne.Activate(.5f);
                break;
        }

    }

    public virtual void Activate(PlayerCamera playerCamera, Vector2 point)
    {
        _sm.Fire(Trigger.Wake);
        playerCamera.Transition(point, playerCamera.screenLimits, 12);
    }


    protected StateMachine<State, Trigger> _sm;

    public void Hit(HitInfo hi)
    {
        CurrentHp -= hi.Damage;
        if (CurrentHp > 0)
        {
            _sm.Fire(Trigger.Hit);
            _sm.Fire(Trigger.NextPhase);

            /*HitstunTimer.Start(hi.HitstunTime);
            NoHit();
            var c = Modulate;
            c.a = 0.3f;
            Modulate = c;
            */
        }
        else
        {
            _sm.Fire(Trigger.Die);
        }
    }

    public bool Persist { get; set; } = true;
    public Vector2 Velocity { get; set; } = Vector2.Zero;
    public float Gravity { get; set; } = 0;
}
