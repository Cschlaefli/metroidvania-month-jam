using Godot;
using System;

public class ShieldPickup : Pickup
{
    public override void AddAbility(Player player)
    {
        base.AddAbility(player);
        player.shield.Known = true;
        label.Text = "Learned Shield, press rmb to reflect projectiles.";
    }
}
