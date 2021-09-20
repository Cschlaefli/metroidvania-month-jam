using Godot;
using System;

public interface IExplodes
{
	bool explodes { get; set; }
	void _explode();
}
