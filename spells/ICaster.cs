using Godot;
using System;

public interface ICaster 
{
    Vector2 Velocity { get; set; }
    float Gravity { get; set; }
    Vector2 Position { get; set; }
    void Recoil(Vector2 recoil)
    {
        Velocity = recoil;
    }
}
