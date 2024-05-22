using Godot;
using System;

public partial class Explosion : Node3D
{
    [Export]
    private GpuParticles3D particles;
    public override void _Ready()
    {
        particles.Emitting = true;
    }
    public void _on_sparks_finished()
    {
        this.QueueFree();
    }
}
