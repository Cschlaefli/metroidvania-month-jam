using Godot;
using System;

public class JumpPickup : Pickup
{
    public override void AddAbility(Player player)
    {
        base.AddAbility(player);
        player.DefaultJumps = -1;
        label.Text = "Learned jump, midair jumps cost mana";
    }
}
