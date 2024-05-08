using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 5/03/2024
/// Manages the games UI overlay, will manage important variables and saving
/// </summary>
public partial class GameManager : Node3D
{
    [Export]
    private ColorRect hitRect;
    private float hitOpacity = 0.33f;

    //reference to the player
    [Export]
    private Player player;

    //reference to the progress bars
    [Export]
    private TextureProgressBar healthBar;
    [Export]
    private TextureProgressBar lipaseBar;

    public override void _Ready()
    {
        healthBar.MaxValue = player.MaxHealth;
    }

    public override void _Process(double delta)
    {
        if (hitRect.Color.A > 0)
        {
            // fade out the red tint effect
            hitRect.Color = new Color(hitRect.Color.R, hitRect.Color.G, hitRect.Color.B, hitRect.Color.A - (float) delta);
        }
    }

    /// <summary>
    /// Roman Noodles
    /// 5/03/2024
    /// Event handler which tints the screen red when the player takes damage
    /// </summary>
    public void _on_player_player_hit()
    {
        hitRect.Color = new Color(hitRect.Color.R, hitRect.Color.G, hitRect.Color.B, hitOpacity);
        healthBar.Value = player.Health;
    }
}
