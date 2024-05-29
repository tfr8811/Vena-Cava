using Godot;
using System;
/// <summary>
/// Roman Noodles
/// 5/22/2024
/// A cop car which drops off a cop then crashes into stuff
/// </summary>
public partial class CopCar : CharacterBody3D
{
    // instance the cop
    private PackedScene psCop = GD.Load<PackedScene>("res://scenes/character/Cop.tscn");
    // instance the explosion
    private PackedScene psExplosion = GD.Load<PackedScene>("res://scenes/effects/Explosion.tscn");
    [Export]
    private Node3D spawnPoint;
    [Export]
    private AudioStreamPlayer3D siren;
    [Export]
    private RayCast3D[] frontRayCasts;
    // cop cars have 1 cop
    private bool hasCop = true;
    public override void _Ready()
    {
        siren.Play();
        Velocity = Vector3.Forward * 20;
    }
    public override void _PhysicsProcess(double delta)
    {
        // face in direction of Velocity
        LookAt(Transform.Origin - Velocity, Vector3.Up);
        MoveAndSlide();

        // handle collisions
        // front raycasts are used to see when the cop car is about to hit something
        // when this happens, the cop jumps out
        for (int i = 0; i < frontRayCasts.Length; i ++)
        {
            if (frontRayCasts[i].IsColliding() && hasCop)
            {
                // the cop car has a node3d which is used to set the position of the cop
                this.SpawnCop(spawnPoint.GlobalPosition);
                hasCop = false;
            }
        }

        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            // the cop car will damage damageable colliders
            Object collider = GetSlideCollision(i).GetCollider();
            if (collider is Node3D)
            {
                if (collider is IDamageable)
                {
                    ((IDamageable)collider).TakeDamage(3);
                }
                // right now characters, trash cans, and even a bullet can blow up the cop car
                if (!((Node3D) collider).IsInGroup("FloorPlane"))
                {
                    this.SpawnExplosion(this.GlobalPosition);
                    this.QueueFree();
                }
            }
        }
    }

    /// <summary>
    /// Roman Noodles
    /// 5/16/2024
    /// Spawns a cop at a spawnpoint
    /// </summary>
    private void SpawnCop(Vector3 spawnPoint)
    {
        Enemy cop = (Enemy)psCop.Instantiate();
        GetNode("/root/World").AddChild(cop);
        cop.GlobalPosition = spawnPoint;
    }

    /// <summary>
    /// Roman Noodles
    /// 5/23/2024
    /// Spawns an explosion at a spawnpoint
    /// </summary>
    private void SpawnExplosion(Vector3 spawnPoint)
    {
        Explosion explosion = (Explosion)psExplosion.Instantiate();
        GetNode("/root/World").AddChild(explosion);
        explosion.GlobalPosition = spawnPoint;
    }
}
