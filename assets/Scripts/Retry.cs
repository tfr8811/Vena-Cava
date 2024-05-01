using Godot;
using System;

public partial class Retry : Button
{
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
        Pressed += OnClick;
    }
    private void OnClick()
    {
        GetTree().ChangeSceneToFile("res://assets/Scenes/MovementTest.tscn");
    }
}
