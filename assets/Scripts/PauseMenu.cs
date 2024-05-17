using Godot;
using System;

/// <summary>
/// Roman Noodles (adapted from KobeDev)
/// 5/17/2024
/// Controls the pause effect and all of the options in the pause menu
/// </summary>
public partial class PauseMenu : Control
{
    // player reference
    private Player player = null;

    private NodePath playerPath = "/root/World/Player";

    [Export]
    HSlider mouseSlider;

    [Export]
    Button resume;

    [Export]
    Button restart;

    [Export]
    Button quit;

    [Export]
    AnimationPlayer animationPlayer;

    [Export]
    Label senseValue;

    public override void _Ready()
    {
        resume.Disabled = true;
        restart.Disabled = true;
        quit.Disabled = true;
        animationPlayer.Play("RESET");
        player = (Player)GetNode(playerPath);
        senseValue.Text = mouseSlider.Value.ToString();
    }
    private void Pause()
    {
        GetTree().Paused = true;
        animationPlayer.Play("Blur");
        resume.Disabled = false;
        restart.Disabled = false;
        quit.Disabled = false;
        // make the mouse visible again for menus
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void Resume()
    {
        GetTree().Paused = false;
        animationPlayer.PlayBackwards("Blur");
        resume.Disabled = true;
        restart.Disabled = true;
        quit.Disabled = true;
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

    public void _on_mouse_slider_drag_ended(bool valueChanged)
    {
        player.Sensitivity = ((float)mouseSlider.Value) / 10000f; 
    }

    public void _on_mouse_slider_value_changed(float value)
    {
        senseValue.Text = value.ToString();
    }
}
