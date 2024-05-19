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

    [Export]
    private Label copCounter;

    private int copsDefeated = 0;
    private bool fullscreen = false;

    public override void _Ready()
    {
        healthBar.MaxValue = player.MaxHealth;

        // setup the copCounter
        copCounter.Text = copsDefeated.ToString();
    }

    public override void _Process(double delta)
    {
        if (hitRect.Color.A > 0)
        {
            // fade out the red tint effect
            hitRect.Color = new Color(hitRect.Color.R, hitRect.Color.G, hitRect.Color.B, hitRect.Color.A - (float)delta);
        }

        // FULLSCREEN Handler
        if (Input.IsActionJustPressed("Fullscreen")) {
            ToggleFullscreen();
        }
    }

    /// <summary>
    /// Roman Noodles
    /// 5/17/2024
    /// Toggles fullscreen
    /// </summary>
    public void ToggleFullscreen()
    {
        // toggle fullscreen
        fullscreen = !fullscreen;
        // switch display mode
        if (fullscreen)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Maximized);
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

    /// <summary>
    /// Roman Noodles
    /// 5/03/2024
    /// Event handler for when the player defeats an enemy
    /// </summary>
    public void _on_player_enemy_defeated()
    {
        copsDefeated += 1;
        copCounter.Text = copsDefeated.ToString();
    }
}
