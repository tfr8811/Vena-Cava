using Godot;
using System;

public partial class HealthPickup : Node3D
{
    [Export]
    private int health;
    public void _on_area_3d_body_entered(Node3D body)
    {
        if (body is Player && ((Player)body).Health < ((Player)body).MaxHealth)
        {
            ((Player)(body)).Heal(health);
            this.QueueFree();
        }
    }
}
