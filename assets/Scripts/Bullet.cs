using Godot;
using System;

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

        // TODO: handle collisions
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            this.QueueFree();
        }

    }
}
