using Godot;
using System;

public interface ICaster 
{
    Vector2 Velocity { get; set; }
    float Gravity { get; set; }
}