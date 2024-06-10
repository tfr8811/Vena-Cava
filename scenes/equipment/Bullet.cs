using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 4/30/2024
/// A shape that the player or enemies can launch at other bodies, damages target
/// </summary>
public partial class Bullet : CharacterBody3D
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

    public override void _PhysicsProcess(double delta)
    {
        // decay the bullet
        bulletTimer -= delta;
        if (bulletTimer < 0) {
            // TODO: object pooling
            this.QueueFree();
        }

        KinematicCollision3D collision = MoveAndCollide(Velocity);
        // handle collisions
        if (collision != null)
        {
            for (int i = 0; i < collision.GetCollisionCount(); i++)
            {
                // the cop car will damage damageable colliders
                GodotObject collider = collision.GetCollider(i);
                if (collider is IDamageable)
                {
                    ((IDamageable)collider).TakeDamage(damage);
                }
                else
                {
                    // Display a bullet hole decal at the collision point
                    Node3D bulletHole = (Node3D)psBulletHole.Instantiate();
                    GetNode("/root/World").AddChild(bulletHole);
                    bulletHole.GlobalPosition = collision.GetPosition();
                    if (collision.GetNormal() != Vector3.Zero)
                    {
                        bulletHole.LookAt(collision.GetPosition() + collision.GetNormal() + new Vector3(0.001f, 0.001f, 0.001f));
                    }
                }
                this.QueueFree();
            }
        }
    }
}
