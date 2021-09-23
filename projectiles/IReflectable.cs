using Godot;
using System;

public interface IReflectable
{
	int ReflectsCount {get; set;}
	int MaxReflects {get;set;}

	Vector2 Direction { get; set; }

	void Reflect(uint newHitmask, Vector2 direction, float speedMod, float damageMod);

}
