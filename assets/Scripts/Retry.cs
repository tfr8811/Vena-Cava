using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 5/01/2024
/// A button which loads the movement scene
/// </summary>
public partial class Retry : Button
{
    [Export]
    AudioStreamPlayer gameOverSound;
    public override void _Ready()
    {
        // make the mouse visible again for menus
        Input.MouseMode = Input.MouseModeEnum.Visible;
        Pressed += OnClick;

        // this is the only script in the game over scene, so it will also play the game over sound
        gameOverSound.Play();

    }
    private void OnClick()
    {
        GetTree().ChangeSceneToFile("res://assets/Scenes/MovementTest2.tscn");
    }
}
