using Godot;
using System;

public interface IReflectable
{
	int reflect_count {get; set;}
	int max_reflects {get;set;}

	void reflect(uint new_hitmask, Vector2 direction);

}
