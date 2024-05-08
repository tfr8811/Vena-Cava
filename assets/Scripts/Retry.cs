using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 5/01/2024
/// A button which loads the movement scene
/// </summary>
public partial class Retry : Button
{
    public override void _Ready()
    {
        // make the mouse visible again for menus
        Input.MouseMode = Input.MouseModeEnum.Visible;
        Pressed += OnClick;
    }
    private void OnClick()
    {
        GetTree().ChangeSceneToFile("res://assets/Scenes/MovementTest.tscn");
    }
}
