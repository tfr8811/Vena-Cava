using func_godot.FGD;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Godot.RenderingDevice;

[Tool]
public partial class LogicToggleRelay : Node, IToggleable, ICsFGD
{
    [Export]
    private Node[] targets;

    [Export]
    private string[] targetNames;

    public void Enable()
    {
        foreach (var target in targets)
        {
            (target as IToggleable).Enable();
        }
    }

    public void Disable()
    {
        foreach (var target in targets)
        {
            (target as IToggleable).Disable();
        }
    }

    public void Toggle()
    {
        foreach (var target in targets)
        {
            (target as IToggleable).Toggle();
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
        targets = TargetUtil.GetNodesByTargetName<IToggleable>(this, targetNames);
    }
}
