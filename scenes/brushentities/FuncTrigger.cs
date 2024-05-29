using func_godot.FGD;
using Godot;
using System;

[Tool]
public partial class FuncTrigger : Area3D, ICsFGD
{
    [Export]
    private string target = null;

    [Export]
    private Node targetNode = null;

    [Export]
    private int limit = -1;

    public override void _Ready()
    {
        BodyEntered += _BodyEntered;
    }

    private void _BodyEntered(Node3D body)
    {
        if (limit == 0) {
            return;
        }

        if (body is Player)
        {
            (targetNode as ITriggerable).Trigger(body);
            limit--;
        }
    }

    public void _func_godot_apply_properties(Godot.Collections.Dictionary<string, Variant> entity_properties)
    {
        if (entity_properties.ContainsKey("limit"))
        {
            limit = entity_properties["limit"].AsInt32();
        }

        if (entity_properties.ContainsKey("target"))
        {
            target = entity_properties["target"].AsString();
        }
    }

    public void _func_godot_build_complete()
    {
        targetNode = TargetUtil.GetNodeByTargetName<ITriggerable>(this, target);
    }
}
