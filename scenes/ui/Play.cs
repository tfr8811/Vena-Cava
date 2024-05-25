using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 5/17/2024
/// A button which loads the level scene
/// </summary>
public partial class Play : Button
{
    public override void _Ready()
    {
        // make the mouse visible for menus
        Input.MouseMode = Input.MouseModeEnum.Visible;
        Pressed += OnClick;
    }
    private void OnClick()
    {
        GlobalSceneManager.Instance.LoadMap("testroom");
    }
}
