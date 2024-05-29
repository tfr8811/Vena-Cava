using func_godot.FGD;
using Godot;
using System;

[Tool]
public partial class LogicTimer : Node, IToggleable, ICsFGD
{
	[Export]
	private string target;

	[Export]
	private Node targetNode;
	
	[Export]
    private double interval = 1;

	[Export]
    private double progress = 0;

	private bool enabled = false;

	public override void _Process(double delta)
	{
		if (enabled)
		{
			progress += delta;

			if (progress > interval)
			{
				progress -= interval;
				(targetNode as ITriggerable).Trigger(this);
			}
		}
	}

	public void Enable()
	{
		enabled = true;
		progress = 0;
	}

    public void Disable()
	{
		enabled = false;
		progress = 0;
	}

	public void Toggle()
	{
		if (!enabled)
		{
			Enable();
		} else
		{
			Disable();
		}
	}

    public void _func_godot_apply_properties(Godot.Collections.Dictionary<string, Variant> entity_properties)
    {
        if (entity_properties.ContainsKey("target"))
        {
            target = entity_properties["target"].AsString();
        }

		if (entity_properties.ContainsKey("interval"))
		{
			interval = entity_properties["interval"].AsDouble();
		}
    }

    public void _func_godot_build_complete()
    {
		targetNode = TargetUtil.GetNodeByTargetName<ITriggerable>(this, target);
    }
}
