using Godot;
using System;

public class HealPickup : Pickup
{
    public override void AddAbility(Player player)
    {
        base.AddAbility(player);
        player.HealKnown = true;
        label.Text = "Learned Heal. Press F when  you have green mana to convert it to health.";
    }
}
