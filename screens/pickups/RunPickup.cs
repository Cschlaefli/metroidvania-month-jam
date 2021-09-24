using Godot;
using System;

public class RunPickup : Pickup
{
    public override void AddAbility(Player player)
    {
        base.AddAbility(player);
        player.RunKnown = true;
        label.Text = "Learned run. Press shift to run.";
    }
}
