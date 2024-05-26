using Godot;
using System;

public partial class GlobalSettings : Node
{
    public static GlobalSettings Instance { get { return instance; } }
    private static GlobalSettings instance;

    private bool fullscreen = false;
    private bool Fullscreen
    {
        get { return fullscreen; }
        set
        {
            GD.Print("Changing fullscreen to: ", fullscreen);
            fullscreen = value;
            if (fullscreen)
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            }
            else
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Maximized);
            }
        }
    }

    private float sensitivity = 0.001f;
    public float Sensitivity
    {
        get
        {
            return this.sensitivity;
        }
        set
        {
            this.sensitivity = value;
        }
    }

    public override void _Ready()
    {
        instance = this;
        // Todo: Load from disk
    }

    /// <summary>
    ///  Handle global keyboard shortcuts like fullscreen
    /// </summary>
    /// <param name="delta"></param>
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Fullscreen"))
        {
            ToggleFullscreen();
        }
    }

    public void ToggleFullscreen()
    {
        Fullscreen = !Fullscreen;
    }

}
