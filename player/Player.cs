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
    public float ExcessMana = 0;

    public float MoveSpeed = Globals.CELL_SIZE * 8;
    public float DefaultMoveSpeed = Globals.CELL_SIZE * 8;
    public float RunSpeed = Globals.CELL_SIZE * 16;

    [Export]
    public bool RunKnown = false;
    [Export]
    public bool HealKnown = false;

    public float JumpHeight = 4;
    public float DoubleJumpHeight = 4;

    public float PlayerAcceleration = 15;
    public float PlayerDeceleration = 30;

    public float TerminalVelocity = Globals.CELL_SIZE * 15;
    const float TERMINAL_VELOCITY = Globals.CELL_SIZE * 15;

    //if <0 infinte Jumps, if == 0 no double jumps if >0 that many air jumps;
    int DefaultJumps = 0;

    int Jumps = 0;
    public float JumpCost = 3;

    public float RunCost = 3;
    int FacingDirection;
    Staff Staff;
    PlayerCamera Cam;

    Timer CayoteTimer;
    Timer CastingTimer;
    Timer RecoveryTimer;

    AudioStreamPlayer2D FootstepSfx;
    AudioStreamPlayer2D JumpSfx;
    AudioStreamPlayer2D CastSfx;

    Node2D SpellNode;

    //TODO:  write these classes
    SpellEquipMenu spellEquipDisplay;
    ResourceDisplay resourceDisplay; 

    [Signal]
    public delegate void SpellListChanged(Array<Spell> equippedSpells);
    [Signal]
    public delegate void ResourcesChanged(ResourceValues rvs);


    Spell CurrentSpell;
    Spell CastingSpell;


    public override void _Ready()
    {
        base._Ready();
        SpellNode = GetNode<Node2D>("SpellList");
        Globals.Player = this;
        foreach(PackedScene sp in SpellList.PlayerSpells)
        {
            var toAdd = sp.Instance<Spell>();
            SpellNode.AddChild(toAdd);
            toAdd.Connect(nameof(Spell.Updated), this, nameof(UpdateSpells));
        }
        spellEquipDisplay = GetNode<SpellEquipMenu>("CanvasLayer/SpellEquipMenu");
        spellEquipDisplay.PlayerSpells = SpellNode.GetChildren();
        spellEquipDisplay.UpdateDisplay();

        UpdateSpells();
    }
    protected void UpdateSpells()
    {

        //TODO: Implement
    }

    protected enum State { Idle, Healing, Run, Jump, Fall, Casting, Recovring, Hitstun, Disabled, Charging}
    protected enum Trigger {}
    protected readonly StateMachine<State, Trigger> _sm = new StateMachine<State, Trigger>(State.Idle);


    public void Hit(HitInfo hi)
    {

    }
}
