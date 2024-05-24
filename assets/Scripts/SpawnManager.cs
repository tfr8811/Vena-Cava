using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 5/16/2024
/// Manages enemy spawn waves
/// </summary>
public partial class SpawnManager : Node
{
    [Export]
    private Spawner[] spawnPoints;
    [Export]
    private Timer spawnTimer;
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    public override void _Ready()
    {
        rng.Randomize();
    }
    public override void _Process(double delta)
    {
        if (spawnTimer.TimeLeft == 0)
        {
            // spawn a cop
            Spawner tempSpawnPoint = spawnPoints[rng.RandiRange(0, spawnPoints.Length-1)];
            tempSpawnPoint.Spawn();
            spawnTimer.Start();
        }
    }
}
