using Godot;
using System;

public partial class BulletHole : Node3D
{
    public void _on_timer_timeout()
    {
        QueueFree();
    }
}
