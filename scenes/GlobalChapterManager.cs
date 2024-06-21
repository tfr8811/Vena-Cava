using Godot;
using System;
using System.Xml.Linq;

public partial class GlobalChapterManager : Node
{
    public static GlobalChapterManager Instance { get { return instance; } }
    private static GlobalChapterManager instance;
    public override void _Ready()
    {
        instance = this;
        // Todo, load game progress from disk
    }

    private Chapter[] chapters = new Chapter[]
            {
            new Chapter() { name = "Chapter 1", description = "The beginning of the adventure.", map = "BridgesOfWestAshley" },
            new Chapter() { name = "Chapter 2", description = "The journey continues.", map = "Map2" },
            new Chapter() { name = "Chapter 3", description = "The epic conclusion.", map = "Map3" }
            };

    private int currentChapter = 0;
    private int unlockedChapter = 0;

    public void StartCurrentChapter()
    {
        Chapter chapterToLoad = chapters[currentChapter];
        GlobalSceneManager.Instance.LoadMap(chapterToLoad.map);
    }
}
