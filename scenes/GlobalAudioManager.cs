using Godot;
using System;

public partial class GlobalAudioManager : Node
{
	public static GlobalAudioManager Instance { get { return instance; } }
    private static GlobalAudioManager instance;

    private AudioStreamPlayer music;
	private AudioStreamPlayer ambient;

	private string currentAmbientTrack = null;
	private string currentMusicTrack = null;

	public override void _Ready()
	{
		instance = this;

		music = new AudioStreamPlayer();
		music.Bus = "Music";
		AddChild(music);
		
		ambient = new AudioStreamPlayer();
		ambient.Bus = "Ambient";
		AddChild(ambient);
	}

	public void PlayAmbient(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			StopAmbient();
		}

		if (path != currentAmbientTrack) {
			currentAmbientTrack = path;
			ambient.Stream = ResourceLoader.Load<AudioStream>(path);
			ambient.Play();
		}
	}

	public void StopAmbient()
	{
		ambient.Stop();
	}
}
