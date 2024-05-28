using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 4/30/2024
/// A shape that the player or enemies can launch at other bodies, damages target
/// </summary>
// todo: change from CharacterBody3D to Raycast3D
public partial class Bullet : RayCast3D
{
    // destroy the bullet after 3 seconds
    private double bulletTimer = 3.0f;
    public double BulletTimer
    {
        set
        {
            this.bulletTimer = value;
        }
    }
    private int damage = 1;
    public int Damage
    {
        set
        {
            this.damage = value;
        }
    }

    private Vector3 velocity;
    public Vector3 Velocity
    {
        set 
        { 
            this.velocity = value;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // apply Velocity
        this.GlobalPosition += velocity * (float) delta;
        // face in direction of Velocity
        LookAt(Transform.Origin - velocity, Vector3.Up);
        // decay the bullet
        bulletTimer -= delta;
        if (bulletTimer < 0) {
            // TODO: object pooling
            this.QueueFree();
        }

        // handle collisions
        if (IsColliding())
        {
            Object collider = GetCollider();

            if (collider is IDamageable)
            {
                ((IDamageable)collider).TakeDamage(damage);
            }
            this.QueueFree();
        }
    }
}
