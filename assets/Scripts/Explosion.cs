using Godot;
using System;
/// <summary>
/// Roman Noodles
/// 5/22/2024
/// Simple explosion effect which damages targets in its range
/// </summary>
public partial class Explosion : Node3D
{
    [Export]
    private GpuParticles3D particles;
    [Export]
    private AudioStreamPlayer3D crashSound;
    public override void _Ready()
    {
        particles.Emitting = true;
        crashSound.Play();
    }
    public void _on_crash_sound_finished()
    {
        this.QueueFree();
    }
}
