using Godot;
using System;

public partial class Status : TextureRect
{

	private TextureProgressBar healthBar;
    private TextureProgressBar lipaseBar;

    public override void _Ready()
	{
		healthBar = GetNode<TextureProgressBar>("HealthBar");
        lipaseBar = GetNode<TextureProgressBar>("LipaseBar");
    }

    public void update(Player player)
    {
        healthBar.MaxValue = player.MaxHealth;
        healthBar.Value = player.Health;
    }
}
