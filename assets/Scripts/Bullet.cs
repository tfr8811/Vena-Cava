using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 4/30/2024
/// A shape that the player or enemies can launch at other bodies, damages target
/// </summary>
// todo: change from CharacterBody3D to Raycast3D
public partial class Bullet : CharacterBody3D
{
    // destroy the bullet after 3 seconds
    private double bulletTimer = 3.0f;

    public override void _PhysicsProcess(double delta)
    {
        // decay the bullet
        bulletTimer -= delta;
        if (bulletTimer < 0) {
            // TODO: object pooling
            this.QueueFree();
        }

        MoveAndSlide();

        // handle collisions
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            // the bullet will damage damageable colliders
            Object collider = GetSlideCollision(i).GetCollider();
            
            if (collider is IDamageable)
            {
                ((IDamageable)collider).TakeDamage(1);
            }
            this.QueueFree();
        }

    }
}
