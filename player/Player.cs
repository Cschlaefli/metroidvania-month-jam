using Godot;
using System;
using Stateless;
using Stateless.Graph;
using Godot.Collections;

public class Player : KinematicBody2D, ICaster, IHitbox
{
    public Vector2 Velocity { get; set; } = Vector2.Zero;
    public float Gravity { get; set; } = 40 * Globals.CELL_SIZE;


    public float Health = 10;
    public float MaxHealth = 10;

    public float HealRate = 2;

    public float Mana = 10;
    public float MaxMana = 10;

    public float ManaRegenRate = 1;
    public float ManaDecayRate = 1;
    public float ExcessMana => Mana - MaxMana;

    public float MoveSpeed = Globals.CELL_SIZE * 8;
    public float DefaultMoveSpeed = Globals.CELL_SIZE * 8;
    public float RunSpeed = Globals.CELL_SIZE * 16;

    [Export]
    public bool RunKnown = false;
    [Export]
    public bool HealKnown = false;

    public float JumpHeight = 4;
    public float DoubleJumpHeight = 4;

    public float PlayerAcceleration = Globals.CELL_SIZE * 32;
    public float PlayerDeceleration = Globals.CELL_SIZE * 16;

    public float TerminalVelocity = Globals.CELL_SIZE * 15;
    const float TERMINAL_VELOCITY = Globals.CELL_SIZE * 15;

    //if <0 infinte Jumps, if == 0 no double jumps if >0 that many air jumps;
    public int DefaultJumps = 0;

    int Jumps = 0;
    public float JumpCost = 3;

    public float RunCost = 3;
    Staff Staff;
    public PlayerCamera Cam;

    Timer CayoteTimer;
    Timer CastingTimer;
    Timer RecoveryTimer;
    Timer IdleTimer;
    Timer HitstunTimer;

    AudioStreamPlayer2D FootstepSfx;
    AudioStreamPlayer2D JumpSfx;
    AudioStreamPlayer2D CastSfx;

    public Node2D SpellNode;

    SpellEquipMenu spellEquipDisplay;
    ResourceDisplay resourceDisplay;

    Array<Spell> EquippedSpells;

    [Signal]
    public delegate void SpellListChanged(Array<Spell> equippedSpells);
    [Signal]
    public delegate void ResourcesChanged(ResourceValues rvs);

    Particles2D RegenParticles;
    Particles2D HealParticles;
    Particles2D DoubleJumpEffect;

    Spell CurrentSpell;
    Spell CastingSpell;
    public Spell shield;
    public Spell Teleport;
    AnimatedSprite AnimatedSprite;


    public override void _Ready()
    {
        base._Ready();
        SpellNode = GetNode<Node2D>("SpellList");

        Staff = GetNode<Staff>("Staff");
        Cam = GetNode<PlayerCamera>("Camera2D");
        CayoteTimer = GetNode<Timer>("CayoteTimer");
        CastingTimer = GetNode<Timer>("CastingTimer");
        RecoveryTimer = GetNode<Timer>("RecoveryTimer");
        IdleTimer = GetNode<Timer>("IdleTimer");
        HitstunTimer = GetNode<Timer>("HitstunTimer");

        FootstepSfx = GetNode<AudioStreamPlayer2D>("Footstep");
        JumpSfx = GetNode<AudioStreamPlayer2D>("Jump");
        CastSfx = GetNode<AudioStreamPlayer2D>("Cast");

        RegenParticles = GetNode<Particles2D>("RegenParticles");
        HealParticles = GetNode<Particles2D>("HealEffect");
        DoubleJumpEffect = GetNode<Particles2D>("DoubleJumpEffect");

        resourceDisplay = GetNode<ResourceDisplay>("CanvasLayer/ResourceDisplay");

        shield = GetNode<Spell>("Shield");
        Teleport = GetNode<Spell>("Teleport");

        AnimatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");

        Teleport.SpriteMat = AnimatedSprite.Material as ShaderMaterial;

        Globals.Player = this;
        foreach(PackedScene sp in SpellList.PlayerSpells)
        {
            var toAdd = sp.Instance<Spell>();
            if(toAdd != null)
            {
                SpellNode.AddChild(toAdd);
                toAdd.Connect(nameof(Spell.Updated), this, nameof(UpdateSpells));
            }
        }

        spellEquipDisplay = GetNode<SpellEquipMenu>("CanvasLayer/SpellEquipMenu");
        spellEquipDisplay.PlayerSpells = SpellNode.GetChildren();
        spellEquipDisplay.UpdateDisplay();
        Connect(nameof(ResourcesChanged), resourceDisplay, nameof(ResourceDisplay.OnResourceChange));
        //Connect(nameof(SpellListChanged), spellEquipDisplay, nameof(SpellEquipMenu.)

        UpdateSpells();
        UpdateResources();

        HitstunTimer.Connect("timeout", this, nameof(EndHitstun));
        RecoveryTimer.Connect("timeout", this, nameof(EndRecovery));
        CastingTimer.Connect("timeout", this, nameof(EndCast));

        shield.Connect(nameof(Shield.Reflected), this, nameof(OnReflected));

        ConfigureStateMachine();
    }
    float ReflectBonus = 1.5f;
    public void OnReflected(Projectile projectile)
    {
        Mana += ReflectBonus;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if(@event.IsActionReleased("jump") && Velocity.y < 0)
        {
            Velocity = new Vector2(Velocity.x, Velocity.y * .3f);
        }

        if(@event.IsActionPressed("shield"))
        {
            _sm.Fire(Trigger.Shield);
        }

        if (@event.IsActionPressed("spell_cycle_forward")) 
        {
            CycleSpell(true);
        }
        if (@event.IsActionPressed("spell_cycle_back")) 
        {
            CycleSpell(false);
        }
        if (@event.IsActionReleased("shoot"))
        {
            _sm.Fire(Trigger.StartCast);
        }
        if (@event.IsActionPressed("heal"))
        {
            _sm.Fire(Trigger.Heal);
        }
        if (@event.IsActionPressed("teleport"))
        {
            _sm.Fire(Trigger.Teleport);
        }
        if (@event.IsActionPressed("jump"))
        {
            if(IsOnFloor() || !CayoteTimer.IsStopped())
            {
                _sm.Fire(Trigger.Jump);
            }
            else
            {
                _sm.Fire(Trigger.DoubleJump);
            }
        }

    }

    protected void CycleSpell(bool forward)
    {
        if (EquippedSpells.Count <= 1) return;
        var end = EquippedSpells.Count - 1;
        if (CurrentSpell != null) CurrentSpell.Guide = false;
        if (forward)
        {
            var temp = EquippedSpells[end];
            EquippedSpells.RemoveAt(end);
            EquippedSpells.Insert(0, temp);
        }
        else
        {
            var temp = EquippedSpells[0];
            EquippedSpells.RemoveAt(0);
            EquippedSpells.Insert(end, temp);
        }
        CurrentSpell = EquippedSpells[0];
        EmitSignal(nameof(SpellListChanged), EquippedSpells);

    }

    public void UpdateSpells()
    {
        EquippedSpells = new Array<Spell>();
        foreach(Node n in SpellNode.GetChildren())
        {
            var sp = n as Spell;
            if ( sp != null && sp.Equipped)
                EquippedSpells.Add(sp);
        }
        if(EquippedSpells.Count > 0)
        {
            CurrentSpell = EquippedSpells[0];
        }
        else
        {
            CurrentSpell = null;
        }
        EmitSignal(nameof(SpellListChanged), EquippedSpells);
        spellEquipDisplay.UpdateDisplay();
    }

    protected enum State { Idle, Healing, Run, Jump, Fall, Casting, Recovering, Hitstun}
    protected enum Trigger { Jump, DoubleJump, Shield, StartCast, Heal, Teleport, Die, Hit,
                                StartRun, StopRun,
                              Idle, Land, EndHitstun, EndRecovery, Cast,
                                StartFalling, Fall}
    protected readonly StateMachine<State, Trigger> _sm = new StateMachine<State, Trigger>(State.Idle);
    protected StateMachine<State, Trigger>.TriggerWithParameters<float> _hitTrigger;

    protected void ConfigureStateMachine()
    {

        _sm.OnUnhandledTrigger((state, trigger) => {
            GD.Print($"Invalid trigger {trigger} in {state}");
            });

        _hitTrigger = _sm.SetTriggerParameters<float>(Trigger.Hit);


        _sm.Configure(State.Idle)
            .OnEntryFrom(Trigger.Land, () => { Jumps = DefaultJumps; Velocity = new Vector2(Velocity.x, 0); })
            .OnEntry(() => IdleTimer.Start())
            .OnExit(() => IdleTimer.Stop())
            .Permit(Trigger.StartFalling, State.Fall)
            .Permit(Trigger.Heal, State.Healing)
            .Permit(Trigger.Hit, State.Hitstun)
            .PermitIf(Trigger.StartCast, State.Casting, () => CanCast(CurrentSpell))
            .PermitIf(Trigger.Shield, State.Casting, () => CanInterruptSpell(shield))
            .PermitIf(Trigger.Teleport, State.Casting, () => CanInterruptSpell(Teleport))
            .Permit(Trigger.Jump, State.Jump)
            .Permit(Trigger.StartRun, State.Run);

        _sm.Configure(State.Healing)
            .OnExit(() => HealParticles.Emitting = false)
            .Permit(Trigger.Idle, State.Idle)
            .Permit(Trigger.Hit, State.Hitstun)
            .PermitIf(Trigger.StartCast, State.Casting, () => CanCast(CurrentSpell))
            .PermitIf(Trigger.Shield, State.Casting, () => CanInterruptSpell(shield))
            .PermitIf(Trigger.Teleport, State.Casting, () => CanInterruptSpell(Teleport))
            .Permit(Trigger.Jump, State.Jump)
            .Permit(Trigger.StartRun, State.Run);

        _sm.Configure(State.Run)
            .OnEntryFrom(Trigger.Land, () => { Jumps = DefaultJumps; Velocity = new Vector2(Velocity.x, 0); })
            .Permit(Trigger.StartFalling, State.Fall)
            .PermitIf(Trigger.StartCast, State.Casting, () => CanCast(CurrentSpell))
            .PermitIf(Trigger.Shield, State.Casting, () => CanInterruptSpell(shield))
            .PermitIf(Trigger.Teleport, State.Casting, () => CanInterruptSpell(Teleport))
            .Permit(Trigger.Hit, State.Hitstun)
            .Permit(Trigger.Jump, State.Jump)
            .Permit(Trigger.StopRun, State.Idle);

        _sm.Configure(State.Jump)
            .OnEntryFrom(Trigger.Jump, () => {
                Velocity = new Vector2(Velocity.x, -Mathf.Sqrt(Gravity * JumpHeight * 2 * Globals.CELL_SIZE));
                CayoteTimer.Start();
            })
            .OnEntryFrom(Trigger.DoubleJump, () => {
                Velocity = new Vector2(Velocity.x, -Mathf.Sqrt(Gravity * JumpHeight * 2 * Globals.CELL_SIZE));
                DoubleJumpEffect.Emitting = true;
                DoubleJumpEffect.Restart();
                Mana -= JumpCost;
                Jumps -= 1; })
            .Permit(Trigger.Fall, State.Fall)
            .PermitIf(Trigger.Land, State.Idle, () => !IsRunning())
            .PermitIf(Trigger.Land, State.Run, () => IsRunning())
            .PermitIf(Trigger.StartCast, State.Casting, () => CanCast(CurrentSpell))
            .PermitIf(Trigger.Shield, State.Casting, () => CanInterruptSpell(shield))
            .PermitIf(Trigger.Teleport, State.Casting, () => CanInterruptSpell(Teleport))
            .Permit(Trigger.Hit, State.Hitstun)
            .PermitReentryIf(Trigger.DoubleJump, () => Jumps > 0 && JumpCost <= Mana);

        _sm.Configure(State.Fall)
            .OnEntryFrom(Trigger.StartFalling, () => CayoteTimer.Start())
            .PermitIf(Trigger.Land, State.Idle, () => !IsRunning())
            .PermitIf(Trigger.Land, State.Run, () => IsRunning())
            .PermitIf(Trigger.StartCast, State.Casting, () => CanCast(CurrentSpell))
            .PermitIf(Trigger.Teleport, State.Casting, () => CanInterruptSpell(Teleport))
            .PermitIf(Trigger.Shield, State.Casting, () => CanInterruptSpell(shield))
            .Permit(Trigger.Hit, State.Hitstun)
            .PermitIf(Trigger.Jump, State.Jump, () => !CayoteTimer.IsStopped())
            .PermitIf(Trigger.DoubleJump, State.Jump, () => Jumps > 0 && JumpCost <= Mana);

        _sm.Configure(State.Casting)
            .OnExit( () => { TerminalVelocity = TERMINAL_VELOCITY; })
            .OnEntryFrom(Trigger.Shield, () => { CastingSpell = shield; EndCast(); })
            .OnEntryFrom(Trigger.Teleport, () => { CastingSpell = Teleport; EndCast(); })
            .OnEntryFrom(Trigger.StartCast, () => { CastingSpell = CurrentSpell; CastingTimer.Start(CastingSpell.CastingTime); })
            .Permit(Trigger.Cast, State.Recovering)
            .Permit(Trigger.Hit, State.Hitstun);

        _sm.Configure(State.Recovering)
            .OnEntry( () => OnCast() )
            .PermitIf(Trigger.EndRecovery, State.Idle, () => IsOnFloor())
            .PermitIf(Trigger.EndRecovery, State.Fall, () => !IsOnFloor())
            .PermitIf(Trigger.Jump, State.Jump, () => IsOnFloor() || !CayoteTimer.IsStopped())
            .PermitIf(Trigger.DoubleJump, State.Jump, () => Jumps > 0 && JumpCost <= Mana);

        _sm.Configure(State.Hitstun)
            .OnEntryFrom(_hitTrigger, t => HitstunTimer.Start(t))
            .PermitIf(Trigger.EndHitstun, State.Idle, () => IsOnFloor())
            .PermitIf(Trigger.EndHitstun, State.Fall, () => !IsOnFloor());

    }
    void EndHitstun() => _sm.Fire(Trigger.EndHitstun);
    void EndRecovery() => _sm.Fire(Trigger.EndRecovery);

    bool IsRunning() => Mathf.Abs(Velocity.x) > 200;

    void OnCast()
    {
        RecoveryTimer.Start(CastingSpell.RecoveryTime);
        CayoteTimer.Stop();
        CastingSpell = null;
    }
    
    void EndCast()
    {
        CastSfx.Play();
        Mana -= CastingSpell.CastingCost;
        var ci = new CastInfo() { By = this, Position = Staff.ProjectileSpawnPosition.GlobalPosition, Direction = Vector2.Up.Rotated(Staff.Rotation) };
        CastingSpell.Cast(ci);
        _sm.Fire(Trigger.Cast);
    }

    bool CanCast(Spell spell)
    {
        return (spell != null && spell.CastingCost <= Mana && spell.Known);
    }

    bool CanInterruptSpell(Spell spell)
    {
        return (CastingSpell == null || CastingSpell.Interruptable) && CanCast(spell);
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        StateLogic(delta);
    }

    public void StateLogic(float delta)
    {
        var s = _sm.State;
        GetNode<Label>("StateLabel").Text = s.ToString();
        HandleGravity(delta);
        HandleWeapon(delta);
        switch (_sm.State)
        {
            case State.Recovering:
                HandleMovement(delta * .05f);
                break;
            case State.Casting:
                var x = Velocity.x;
                var y = Velocity.y;
                Velocity = Velocity.LinearInterpolate(new Vector2(0, 0), delta);
                break;
            case State.Healing:
                HandleHealing(delta);
                break;
            case State.Idle:
                HandleMovement(delta);
                RegenMana(delta);
                if (!IsOnFloor())
                {
                    if (Velocity.y > 200)
                    {
                        _sm.Fire(Trigger.StartFalling);
                    }
                }else if(IsRunning())
                {
                    _sm.Fire(Trigger.StartRun);
                }
                break;
            case State.Run:
                HandleMovement(delta);
                RegenMana(delta);
                if (!IsOnFloor())
                {
                    if (Velocity.y > 200)
                    { 
                        _sm.Fire(Trigger.StartFalling);
                    }
                }else if(!IsRunning())
                {
                    _sm.Fire(Trigger.StopRun);
                }
                break;
            case State.Jump:
                HandleMovement(delta);
                RegenMana(delta);
                if (Velocity.y > 120) 
                { 
                    _sm.Fire(Trigger.Fall);
                }else if (IsOnFloor() && CayoteTimer.IsStopped()) {
                    _sm.Fire(Trigger.Land);
                }
                break;
            case State.Fall :
                HandleMovement(delta);
                RegenMana(delta);
                if (IsOnFloor())
                {
                    _sm.Fire(Trigger.Land);
                }
                break;
            default:
                HandleMovement(delta);
                HandleJumping(delta);
                if(CurrentSpell != null && CurrentSpell.Charging)
                {
                    RegenMana(delta * .5f);
                }
                else { RegenMana(delta); }
                break;
        }
        HandleCamera(delta);
        ApplyVelocity();
        UpdateResources();
        
    }
    void ApplyVelocity()
    {
        MoveAndSlide(Velocity, Vector2.Up);
    }
    void UpdateResources()
    {
        var rvs = new ResourceValues() { Health = Health, ExcessMana = ExcessMana, Mana = Mana, MaxHealth = MaxHealth, MaxMana = MaxMana };
        EmitSignal(nameof(ResourcesChanged), rvs);
    }

    public Vector2 CastDirection = Vector2.Zero;
    protected void RegenMana(float delta)
    {
        //make this a substate
        var manaRegen = ManaRegenRate;
        if(_sm.State == State.Idle && IdleTimer.IsStopped())
        {
            manaRegen *= 3;
            RegenParticles.Emitting = true;
            if (Mana >= MaxMana - .05) {
                RegenParticles.Emitting = false;
            }
        }else{
            RegenParticles.Emitting = false;
        }

        if(ExcessMana < 0)
        {
            Mana += manaRegen * delta  + ((MaxMana/Mana)- .9f)  * delta;
            //Mana += manaRegen  * delta;
        }else if(ExcessMana > 0)
        {
            Mana -= ManaDecayRate * delta;
            if (ExcessMana < 0) Mana = MaxMana;
        }
        UpdateResources();

    }
    protected void HandleHealing(float delta)
    {
        if(ExcessMana > 0 && Health < MaxHealth && Input.IsActionPressed("heal"))
        {
            var rate = HealRate * delta;
            Mana -= rate;
            Health += rate;
        }
        else
        {
            _sm.Fire(Trigger.Idle);
        }

    }
    protected void HandleWeapon(float delta)
    {
        if (CurrentSpell != null && Input.IsActionPressed("shoot"))
        {
            CurrentSpell.Guide = true;
            if (CurrentSpell.Chargeable) CurrentSpell.Charging = true;
            CurrentSpell.CanCast = CurrentSpell.CastingCost <= Mana;
            resourceDisplay.ShowCost(CurrentSpell.CastingCost, CurrentSpell.CanCast);
        }
        else { 
            if(CurrentSpell != null)
            {
                CurrentSpell.Guide = false;
                CurrentSpell.Charging = false;
            }
            resourceDisplay.ShowCost(0, true);
        }

        if(!Globals.MouseAim)
        {
            var x = 0f;
            var y = 0f;
            x = Input.GetActionStrength("aim_right") - Input.GetActionStrength("aim_left");
            y = Input.GetActionStrength("aim_down") - Input.GetActionStrength("aim_up");
            if(x+y != 0)
            {
                CastDirection = new Vector2(x, y).Normalized();
            }
        }
        else
        {
            CastDirection = (GetGlobalMousePosition() - GlobalPosition).Normalized();
        }
        float rot = CastDirection.Angle() + (float)Math.PI * .5f;
        if(_sm.State == State.Casting)
        {
            Staff.Rotation = Mathf.LerpAngle(Staff.Rotation, rot, delta * 3);
        }
        else
        {
            Staff.Rotation = Mathf.LerpAngle(Staff.Rotation, rot, delta * 10);
        }
        CastDirection = new Vector2(Mathf.Cos(Staff.Rotation), Mathf.Sin(Staff.Rotation)).Rotated((float)Math.PI * -.5f);
    }
    const float LookDistanceY = Globals.CELL_SIZE * 3;
    const float LookDistanceX = Globals.CELL_SIZE * 3;
    protected void HandleCamera(float delta)
    {
        var MousePos = GetLocalMousePosition();
        float x;
        float y;
        var x_dist = Mathf.Clamp(MousePos.x, -LookDistanceX, LookDistanceX);
        var y_dist = Mathf.Clamp(MousePos.y, -LookDistanceY, LookDistanceY);
        if (Globals.MouseAim)
        {
            x = Mathf.Clamp(Mathf.Abs(MousePos.x), 1, 1000) * Mathf.Sign(MousePos.x) / 1000;
            y = Mathf.Clamp(Mathf.Abs(MousePos.y), 1, 1000) * Mathf.Sign(MousePos.y) / 1000;
        }
        else
        {
            x = Input.GetActionStrength("aim_right") - Input.GetActionStrength("aim_left");
            y = Input.GetActionStrength("aim_down") - Input.GetActionStrength("aim_up");
            x_dist = LookDistanceX * x;
            y_dist = LookDistanceY * y;
        }
        y = Mathf.Clamp(Mathf.Abs(y * 10), .5f, 2);
        x = Mathf.Clamp(Mathf.Abs(x * 10), 2, 4);
        var vect = new Vector2(x, y);
        Cam.Position = Cam.Position.LinearInterpolate(vect, delta * vect.Length());
    }

    protected void HandleJumping(float delta)
    {
        if (Input.IsActionJustPressed("jump"))
        {
            if(IsOnFloor() || !CayoteTimer.IsStopped())
            {
                _sm.Fire(Trigger.Jump);
            }
            else
            {
                _sm.Fire(Trigger.DoubleJump);
            }
        }
    }


    protected void HandleGravity(float delta)
    {
        var y = Velocity.y;
        var x = Velocity.x;
        if (IsOnCeiling())
        {
            y += Globals.CELL_SIZE;
        }
        if (IsOnFloor())
        {
            /*
            if(_sm.State != State.Hitstun)
            {
                y = Mathf.Lerp(y, 0, delta * 15);
            }*/
        }else if(y <= TerminalVelocity)
        {
            y += Gravity * delta;
        }
        else
        {
            y -= Gravity * delta * .5f;
        }
        Velocity = new Vector2(x, y);
    }
    protected void HandleMovement(float delta) 
    {
        var x = Velocity.x;
        float lerpVal = 0;
        var accel = PlayerAcceleration;
        if(Mathf.Abs(x) > MoveSpeed || Mathf.Abs(x) < MoveSpeed * .5 ) { accel *= 2; }

        if (Input.IsActionPressed("move_right"))
        {
            if (x < 0) accel *= 2;            
            lerpVal = Helpers.Accelerate(x, MoveSpeed, accel, delta);
        }else if (Input.IsActionPressed("move_left"))
        {
            if (x > 0) accel *= 2;            
            lerpVal = Helpers.Accelerate(x, -MoveSpeed, accel, delta);
        }else if (IsOnWall())
        {
            lerpVal = 0;
        }else     
        {
            accel = PlayerDeceleration;
            if(Mathf.Abs(x) < MoveSpeed * .75 ) { accel *= 8; }
            lerpVal = Helpers.Accelerate(x, 0, accel, delta);
        }

        Velocity = new Vector2(lerpVal, Velocity.y);
    }


    public void Die()
    {
        //TODO player test
    }

    public void Hit(HitInfo hi)
    {
        Health -= hi.Damage;
        if( Health > 0)
        {
            var diff = GlobalPosition - hi.By.GlobalPosition;
            diff = new Vector2(Math.Sign(diff.x), 1);

            Velocity = hi.Knockback * diff;
            _sm.Fire(Trigger.Hit);
        }
        else
        {
            _sm.Fire(Trigger.Die);
        }
    }
}
