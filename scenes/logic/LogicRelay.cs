using func_godot.FGD;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Godot.RenderingDevice;

[Tool]
public partial class LogicRelay : Node, ITriggerable, ICsFGD
{
    [Export]
    private Node[] targets;

    [Export]
    private string[] targetNames;

    public void Trigger(Node node)
    {
        foreach (var target in targets)
        {
            (target as ITriggerable).Trigger(target);
        }
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
