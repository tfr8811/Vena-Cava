using func_godot.FGD;
using Godot;
using System;
using static Godot.RenderingServer;

[Tool]
public partial class EnvTone : Area3D, ICsFGD
{
	[Export]
	private String path = null;

	public override void _Ready()
	{
        BodyEntered += _BodyEntered; 
	}

    private void _BodyEntered(Node3D body)
    {
        if (body is Player)
        {
            play();
        }
    }

	public void play()
	{
		GlobalAudioManager.Instance.PlayAmbient(path);
	}

	public void _func_godot_apply_properties(Godot.Collections.Dictionary<string, Variant> entity_properties)
	{
        if (entity_properties.ContainsKey("path"))
        {
            path = entity_properties["path"].AsString();
        }
    }

    public void _func_godot_build_complete()
    {
		
    }
}