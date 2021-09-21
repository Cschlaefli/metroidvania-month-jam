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
    double MaxHp { get; set; } = 0;
    [Export]
    double Gravity { get; set; } = 8;
    [Export] 
    double TerminalVelocity { get; set; } = 0;
    [Export]
    int ManaDropped { get; set; } = 1;
    [Export]
    double ManaValue { get; set; } = 5.0;
    [Export]
    float ShootRand { get; set; } = 1;
    [Export]
    float ShootInterval { get; set; } = 2;
    int CurrentHp;

    KinematicBody2D CurrentEnemy;
    Area2D Hurtbox;
    Timer FearTimer;
    Timer CastTimer;
    Timer RecoveryTimer;
    Timer ShootTimer;
    Timer FrozenTimer;
    RayCast2D LineOfSight;

    double PlayerDistance = 1;
    Vector2 PlayerDirection = Vector2.Zero;
    bool CanSeePlayer = false;
    [Signal]
    delegate void Die();

    Spell CastingSpell;



/*
var velocity = Vector2.ZERO

export var max_hp := 1.0
export var gravity = 8
export var terminal_velocity = 5.0
export var mana_dropped := 1
export var mana_value := 5.0
onready var mana := preload('res://enemies/ManaPellet.tscn')
export var shoot_rand := .1
export var shoot_interval := 2.0
var hp := max_hp

var dead := false
var ENEMY : PackedScene
onready var curr_enemy : KinematicBody2D = $EnemyBody
onready var hurtbox := $EnemyBody/Hurtbox
onready var fear_timer := $FearTimer
onready var hitstun_timer := $HitstunTimer
onready var casting_timer := $CastTimer
onready var recovery_timer := $RecoveryTimer
onready var shoot_timer := $ShootTimer
onready var frozen_timer := $FrozenTimer
onready var line_of_sight := $EnemyBody/LineOfSight

var player_dist := 1.0
var player_dir := Vector2.ZERO
var sees_player := false

var casting_spell : Spell

signal die
*/

    enum State { Disabled, Hitstun, Idle, Agro, Casting, Recovery, Dead }
    enum Trigger {  Hit, Die, Cast, Respawn, Recover, Wake, Agro, Sleep, Forget }
    enum StatusState {  None, Frozen, Fear}
    enum StatusTrigger { None, Freeze, Afraid }

    readonly StateMachine<State, Trigger> _sm = new StateMachine<State, Trigger>(State.Disabled);
    readonly StateMachine<StatusState, StatusTrigger> StatusStateMachine = new StateMachine<StatusState, StatusTrigger>(StatusState.None);
    public override void _Ready()
    {
        _sm.OnUnhandledTrigger((state, trigger) => {
            GD.Print($"Invalid trigger {trigger} in {state}");
            });
        var _hitTrigger = _sm.SetTriggerParameters<HitInfo>(Trigger.Hit);

        _sm.Configure(State.Disabled)
            .InternalTransition(Trigger.Respawn, t => _Respawn())
            .Permit(Trigger.Wake, State.Idle);

        _sm.Configure(State.Dead)
            .Permit(Trigger.Respawn, State.Idle)
            .Permit(Trigger.Sleep, State.Disabled);

        _sm.Configure(State.Hitstun)
            .OnEntryFrom(_hitTrigger, hi => Hit(hi))
            .Permit(Trigger.Recover, State.Agro)
            .Permit(Trigger.Sleep, State.Disabled);

        _sm.Configure(State.Idle)
            .Permit(Trigger.Hit, State.Hitstun)
            .Permit(Trigger.Cast, State.Casting)
            .Permit(Trigger.Die, State.Dead)
            .Permit(Trigger.Agro, State.Agro)
            .Permit(Trigger.Sleep, State.Disabled);

        _sm.Configure(State.Casting)
            .Permit(Trigger.Cast, State.Recovery)
            .PermitIf(Trigger.Hit, State.Hitstun, () => CanCastInterrupt())
            .Permit(Trigger.Die, State.Dead)
            .Permit(Trigger.Sleep, State.Disabled);

        _sm.Configure(State.Recovery)
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
        return true;
    }

    public virtual void _Respawn()
    {

    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        _StateLogic(delta);
    }

    public virtual void _StateLogic(float delta)
    {
        switch (_sm.State)
        {

        }
    }

    public void Hit(HitInfo hi)
    {

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
