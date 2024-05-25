using Godot;
using System;
using static Godot.RenderingServer;

public partial class Door : AnimatableBody3D, IInteractable
{
	private double progress = 0f;

	private double timeToOpen = .5;

	private bool locked = false;

	private bool open = false;

    private AudioStreamPlayer3D audioStreamPlayer3D;

    private AudioStream moveSound;

    private AudioStream stopSound;


    public override void _Ready()
    {
        audioStreamPlayer3D = new AudioStreamPlayer3D();
        this.AddChild(audioStreamPlayer3D);
        this.moveSound = ResourceLoader.Load<AudioStream>("res://assets/Audio/SFX/Door/door1_move.wav");
        this.stopSound = ResourceLoader.Load<AudioStream>("res://assets/Audio/SFX/Door/door1_stop.wav");
    }

    public void Interact()
	{
		if (locked) return;

        audioStreamPlayer3D.Stream = moveSound;
        audioStreamPlayer3D.Play();

        open = !open;
	}


	public override void _PhysicsProcess(double delta)
	{
		double speed = 1.0 / timeToOpen * delta;

		if (open && progress < 1.0) {
			progress = Math.Min(1.0, progress + speed);
		    if (progress == 1.0)
            {
                audioStreamPlayer3D.Stream = stopSound;
                audioStreamPlayer3D.Play();
            }
        } else if (!open && progress > 0.0) {
			progress = Math.Max(0.0, progress - speed);
            if (progress == 0)
            {
                audioStreamPlayer3D.Stream = stopSound;
                audioStreamPlayer3D.Play();
            }
        }
		Rotation = new Vector3(0, (float)(Math.Tau / 4.0 * progress), 0);
	}
}
