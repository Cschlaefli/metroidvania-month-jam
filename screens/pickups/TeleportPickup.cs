using Godot;
using System;

public class TeleportPickup : Pickup
{
    public override void AddAbility(Player player)
    {
        base.AddAbility(player);

        player.Teleport.Known = true;
        label.Text = "Learned Teleport. Press ctrl + a move direction to teleport.";
    }
}
