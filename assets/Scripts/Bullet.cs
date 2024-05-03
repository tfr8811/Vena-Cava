using Godot;
using System;

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
