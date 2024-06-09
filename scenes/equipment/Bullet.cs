using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 4/30/2024
/// A shape that the player or enemies can launch at other bodies, damages target
/// </summary>
public partial class Bullet : RayCast3D
{
    [Export]
    PackedScene psBulletHole;
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
        // set target position
        this.TargetPosition = velocity * (float) delta;

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
            } else
            {
                // Display a bullet hole decal at the collision point
                Node3D bulletHole = (Node3D)psBulletHole.Instantiate();
                GetNode("/root/World").AddChild(bulletHole);
                bulletHole.GlobalPosition = GetCollisionPoint();
                if (GetCollisionNormal() != Vector3.Zero)
                {
                    bulletHole.LookAt(GetCollisionPoint() + GetCollisionNormal() + new Vector3(0.001f, 0.001f, 0.001f));
                }
            }
            this.QueueFree();
        }

        // apply Velocity
        this.Position += velocity * (float)delta;
    }
}
