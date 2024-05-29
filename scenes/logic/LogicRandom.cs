using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Godot.RenderingDevice;

[Tool]
public partial class LogicRandom : Node, ITriggerable
{
    [Export]
	private Node[] targets;

    [Export]
    private string[] targetNames;

    public void Trigger(Node node)
    {
        if (targets.Length == 0)
            return;

        int choice = (int)(GD.Randi() % targets.Length);
        
        Node target = targets[choice];
        (target as ITriggerable).Trigger(target);
    }

    public void _func_godot_apply_properties(Godot.Collections.Dictionary<string, Variant> entity_properties)
    {
        if (entity_properties.ContainsKey("targets"))
        {
            string targetsString = entity_properties["targets"].AsString();
            targetNames = targetsString.Split(';');
        }
    }

    public void _func_godot_build_complete()
    {
        targets = TargetUtil.GetNodesByTargetName<ITriggerable>(this, targetNames);
    }
}
