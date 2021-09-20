using Godot;
using System;
using Stateless;

public class Enemy : Node2D, IHitbox
{
    enum State { Disabled, Hitstun, Idle, Agro, Casting, Recovery, Dead }
    enum Trigger {  Hit, Die, Cast, Respawn, Recover, Wake, Agro, Sleep }
    enum StatusState {  None, Frozen, Fear}
    enum StatusTrigger { None, Freeze, Afraid }

    readonly StateMachine<State, Trigger> _sm = new StateMachine<State, Trigger>(State.Disabled);
    StateMachine<State, Trigger>.TriggerWithParameters<HitInfo> _hit;
    readonly StateMachine<StatusState, StatusTrigger> StatusStateMachine = new StateMachine<StatusState, StatusTrigger>(StatusState.None);
    public override void _Ready()
    {
        _sm.Configure(State.Disabled)
            .InternalTransition(Trigger.Respawn, t => _respawn())
            .Permit(Trigger.Wake, State.Idle);

        _sm.Configure(State.Dead)
            .Permit(Trigger.Respawn, State.Idle)
            .Permit(Trigger.Sleep, State.Disabled);

        _sm.Configure(State.Hitstun)
            .Permit(Trigger.Recover, State.Agro)
            .Permit(Trigger.Sleep, State.Disabled);

        _sm.Configure(State.Idle)
            .Permit(Trigger.Hit, State.Hitstun)
            .Permit(Trigger.Cast, State.Casting)
            .Permit(Trigger.Die, State.Dead)
            .Permit(Trigger.Agro, State.Agro)
            .Permit(Trigger.Sleep, State.Disabled);
    }
    public virtual void _respawn()
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
}
