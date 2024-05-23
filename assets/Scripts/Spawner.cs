using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 5/23/2024
/// Manages enemy spawn
/// </summary>
public partial class Spawner : Node3D
{
    [Export]
    private PackedScene psCharacter;
    [Export]
    private Vector3 velocity;

    /// <summary>
    /// Roman Noodles
    /// 5/23/2024
    /// Spawns a character at a spawnpoint
    /// </summary>
    public void Spawn()
    {
        CharacterBody3D character = (CharacterBody3D)psCharacter.Instantiate();
        character.GlobalPosition = this.GlobalPosition;
        GetNode("/root").AddChild(character);
        if (velocity.Length() > 0.1)
        {
            character.Velocity = velocity;
        }
    }
}
