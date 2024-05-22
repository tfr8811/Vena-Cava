using Godot;
using System;

public partial class CopCar : CharacterBody3D
{
    // instance the cop
    PackedScene psCop = GD.Load<PackedScene>("res://assets/Scenes/Cop.tscn");
    public override void _Ready()
    {

    }
    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();

        // handle collisions
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            // the bullet will damage damageable colliders
            Object collider = GetSlideCollision(i).GetCollider();
            if (collider is Node3D)
            {
                if (collider is IDamageable)
                {
                    ((IDamageable)collider).TakeDamage(3);
                }
                if (!((Node3D) collider).IsInGroup("FloorPlane") && collider is StaticBody3D)
                {
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
        GetNode("/root").AddChild(cop);
        cop.GlobalPosition = spawnPoint;
    }
}
