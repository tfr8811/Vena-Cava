using Godot;
using System;

/// <summary>
/// Roman Noodles (adapted from KobeDev)
/// 5/17/2024
/// Controls the pause effect and all of the options in the pause menu
/// </summary>
public partial class PauseMenu : Control
{
    // Game Manager Reference
    private GameManager gameManager = null;

    private NodePath gameManagerPath = "/root/World";

    // player reference
    private Player player = null;

    private NodePath playerPath = "/root/World/Player";

    [Export]
    HSlider mouseSlider;

    [Export]
    Button bResume;

    [Export]
    Button bRestart;

    [Export]
    Button bQuit;

    [Export]
    Button bFullscreen;

    [Export]
    AnimationPlayer animationPlayer;

    [Export]
    Label senseValue;

    public override void _Ready()
    {
        bResume.Disabled = true;
        bRestart.Disabled = true;
        bQuit.Disabled = true;
        bFullscreen.Disabled = true;
        animationPlayer.Play("RESET");
        gameManager = (GameManager)GetNode(gameManagerPath);
        player = (Player)GetNode(playerPath);
        senseValue.Text = mouseSlider.Value.ToString();
    }
    private void Pause()
    {
        GetTree().Paused = true;
        animationPlayer.Play("Blur");
        bResume.Disabled = false;
        bRestart.Disabled = false;
        bQuit.Disabled = false;
        bFullscreen.Disabled = false;
        // make the mouse visible again for menus
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void Resume()
    {
        GetTree().Paused = false;
        animationPlayer.PlayBackwards("Blur");
        bResume.Disabled = true;
        bRestart.Disabled = true;
        bQuit.Disabled = true;
        bFullscreen.Disabled = true;
        // return mouse mode to captured for gameplay
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Pause") && !GetTree().Paused)
        {
            Pause();
        }
        else if (Input.IsActionJustPressed("Pause") && GetTree().Paused)
        {
            Resume();
        }
    }

    public void _on_resume_pressed()
    {
        Resume();
    }

    public void _on_restart_pressed()
    {
        Resume();
        // delete all enemies - prevents ghosts
        var allEnemies = GetTree().GetNodesInGroup("Enemies");
        foreach (var enemy in allEnemies)
        {
            enemy.QueueFree();
        }
        // reload current scene gives console errors atm
        GetTree().ReloadCurrentScene();
    }

    public void _on_quit_pressed()
    {
        Resume();
        // delete all enemies - prevents ghosts
        var allEnemies = GetTree().GetNodesInGroup("Enemies");
        foreach (var enemy in allEnemies)
        {
            enemy.QueueFree();
        }
        // quit to title
        GetTree().ChangeSceneToFile("res://assets/Scenes/TitleScreen.tscn");
    }

    public void _on_fullscreen_pressed()
    {
        gameManager.ToggleFullscreen();
    }

    public void _on_mouse_slider_drag_ended(bool valueChanged)
    {
        player.Sensitivity = ((float)mouseSlider.Value) / 10000f; 
    }

    public void _on_mouse_slider_value_changed(float value)
    {
        senseValue.Text = value.ToString();
    }
}
