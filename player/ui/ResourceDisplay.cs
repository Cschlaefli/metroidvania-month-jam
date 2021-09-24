using Godot;
using System;

public class ResourceValues : Godot.Object
{
	public float Health { get; set; }
	public float MaxHealth { get; set; }
	public float Mana { get; set; }
	public float MaxMana { get; set; }
	public float ExcessMana { get; set; } = 0;
}

public class ResourceDisplay : Node
{
	ProgressBar HealthBar;
	ProgressBar ManaBar;
	ProgressBar ExcessBar;
	ShaderMaterial CostMat;
	ResourceValues rvs;
	bool CanCast = true;

    public override void _Ready()
    {
        base._Ready();
		rvs = new ResourceValues() { Health = 10, MaxHealth = 10, ExcessMana = 0, Mana = 10, MaxMana = 10 };
		HealthBar = GetNode<ProgressBar>("HealthBar");
		ManaBar = GetNode<ProgressBar>("ManaBar");
		ExcessBar = GetNode<ProgressBar>("ExcessBar");
		CostMat = GetNode<ColorRect>("CostBar").Material as ShaderMaterial;

    }
    public void ShowCost(float cost, bool canCast)
    {
		CostMat.SetShaderParam("can_cast", canCast);
		CostMat.SetShaderParam("cost", cost);
		if(rvs.ExcessMana < cost)
        {
			CostMat.SetShaderParam("max_mana", rvs.MaxMana);
			CostMat.SetShaderParam("curr", ManaBar.Value + ExcessBar.Value);
        }
        else
        {
			CostMat.SetShaderParam("max_mana", rvs.MaxMana * 2);
			CostMat.SetShaderParam("curr", ExcessBar.Value);
        }
    }
	public void OnResourceChange(ResourceValues _rvs) 
	{
		rvs = _rvs;

		//Add another bar
		if (ManaBar.MaxValue != rvs.MaxMana) ManaBar.MaxValue = rvs.MaxMana;
		if (HealthBar.MaxValue != rvs.MaxHealth) HealthBar.MaxValue = rvs.MaxHealth;
		if (ExcessBar.MaxValue != rvs.MaxMana * 2) ExcessBar.MaxValue = rvs.MaxMana * 2;
	}
    public override void _Process(float delta)
    {
        base._Process(delta);

		ExcessBar.Value = Mathf.Lerp((float)ExcessBar.Value, rvs.ExcessMana, delta * (5 + Mathf.Abs((float)ExcessBar.Value - rvs.ExcessMana)));
		ManaBar.Value = Mathf.Lerp((float)ManaBar.Value, rvs.Mana, delta * (5 + Mathf.Abs((float)ManaBar.Value - rvs.Mana)));
		HealthBar.Value = Mathf.Lerp((float)HealthBar.Value, rvs.Health, delta * (5 + Mathf.Abs((float)HealthBar.Value - rvs.Health)));
    }

}
