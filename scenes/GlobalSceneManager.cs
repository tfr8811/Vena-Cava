using Godot;
using System;

public partial class GlobalSceneManager: Node
{
    public static GlobalSceneManager Instance { get { return instance; } }
    private static GlobalSceneManager instance;

    public static PackedScene LoadingScreen;
    public static String NextScene = string.Empty;

    private string currentScene = string.Empty;

    const string MAIN_MENU_PATH = "res://scenes/menus/TitleScreen.tscn";
    const string GAME_OVER_PATH = "res://scenes/menus/GameOver.tscn";
    const string LOAD_SCREEN_PATH = "res://scenes/menus/LoadingScreen.tscn";

    public override void _Ready()
    {
        instance = this;
        LoadingScreen = (PackedScene)ResourceLoader.Load(LOAD_SCREEN_PATH);
        NextScene = (MAIN_MENU_PATH);
    }


    public void MainMenu()
    {
        SetScene(MAIN_MENU_PATH);
    }

    public void GameOver()
    {
        SetScene(GAME_OVER_PATH);
    }

    public void SetScene(string path)
    {
        if (!ResourceLoader.Exists(path)) {
            GD.PushWarning(String.Format("Failed to load scene: \"{0}\"; not found."), path);
            return;
        }
        NextScene = path;
        GetTree().ChangeSceneToPacked(LoadingScreen);
    }

    public void LoadMap(string mapName)
    {
        GD.Print(String.Format("Loading map: \"{0}\"", mapName));
        SetScene("res://maps/" + mapName + ".tscn");
    }
}
