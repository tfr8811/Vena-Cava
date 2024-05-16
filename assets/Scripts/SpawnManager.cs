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
    private Node3D[] spawnPoints;
    [Export]
    private Timer spawnTimer;
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    // instance the cop
    PackedScene psCop = GD.Load<PackedScene>("res://assets/Scenes/Cop.tscn");
    public override void _Ready()
    {
        rng.Randomize();
    }
    public override void _Process(double delta)
    {
        if (spawnTimer.TimeLeft == 0)
        {
            // spawn a cop
            Node3D tempSpawnPoint = spawnPoints[rng.RandiRange(0, spawnPoints.Length-1)];
            SpawnCop(tempSpawnPoint);
            spawnTimer.Start();
        }
    }

    /// <summary>
    /// Roman Noodles
    /// 5/16/2024
    /// Spawns a cop at a spawnpoint
    /// </summary>
    private void SpawnCop(Node3D spawnPoint)
    {
        Enemy cop = (Enemy)psCop.Instantiate();
        GetNode("/root").AddChild(cop);
        cop.GlobalPosition = spawnPoint.GlobalPosition;
    }
}
