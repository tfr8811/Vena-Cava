using Godot;
using System;

public partial class GlobalSceneManager: Node
{
    public static GlobalSceneManager Instance { get { return instance; } }
    private static GlobalSceneManager instance;

    private string currentScene = string.Empty;

    public override void _Ready()
    {
        instance = this;
    }

    public void SetScene(string path)
    {
        if (!ResourceLoader.Exists(path)) {
            GD.PushWarning(String.Format("Failed to load scene: \"{0}\"; not found."), path);
            return;
        }

        GetTree().ChangeSceneToFile(path);
    }

    public void LoadMap(string mapName)
    {
        GD.Print(String.Format("Loading map: \"{0}\"", mapName));
        SetScene("res://maps/" + mapName + ".tscn");
    }
}
