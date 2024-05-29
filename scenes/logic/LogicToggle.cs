using func_godot.FGD;
using Godot;
using System;

[Tool]
public partial class LogicToggle : Node, ITriggerable, ICsFGD
{
    [Export]
    private string target = null;

    [Export]
    private Node targetNode = null;

    public void Trigger(Node node)
    {
        (targetNode as IToggleable).Toggle();
    }

    public void _func_godot_apply_properties(Godot.Collections.Dictionary<string, Variant> entity_properties)
    {
        if (entity_properties.ContainsKey("target"))
        {
            target = entity_properties["target"].AsString();
        }
    }

    public void _func_godot_build_complete()
    {
        targetNode = TargetUtil.GetNodeByTargetName<IToggleable>(this, target);
    }
}
