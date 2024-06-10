using Godot;
using System;

public partial class LoadingScreen : Control
{
    [Export]
    ProgressBar progressBar;

    public override void _Ready()
    {
        ResourceLoader.LoadThreadedRequest(GlobalSceneManager.NextScene);
    }

    public override void _Process(double delta)
    {
        Godot.Collections.Array progress = new Godot.Collections.Array();
        ResourceLoader.LoadThreadedGetStatus(GlobalSceneManager.NextScene, progress);
        progressBar.Value = (double)progress[0] * 100;

        if ((double)progress[0] == 1.0)
        {
            PackedScene packedScene = (PackedScene)ResourceLoader.LoadThreadedGet(GlobalSceneManager.NextScene);
            GetTree().ChangeSceneToPacked(packedScene);
        }
    }
}
