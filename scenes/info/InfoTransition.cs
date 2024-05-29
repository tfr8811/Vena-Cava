using func_godot.FGD;
using Godot;
using System;
using static Godot.RenderingServer;

[Tool]
public partial class InfoTransition : Area3D, ICsFGD
{
	[Export]
	private String mapName = null;

	public override void _Ready()
	{
        BodyEntered += _BodyEntered;

        SetCollisionMaskValue(1, false);
        SetCollisionMaskValue(2, true);
    }

    private void _BodyEntered(Node3D body)
    {
        GlobalSceneManager.Instance.LoadMap(mapName);
    }


	public void _func_godot_apply_properties(Godot.Collections.Dictionary<string, Variant> entity_properties)
	{
        if (entity_properties.ContainsKey("mapname"))
        {
            mapName = entity_properties["mapname"].AsString();
        }
    }

    public void _func_godot_build_complete()
    {
		
    }
}