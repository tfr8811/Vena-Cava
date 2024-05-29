using Godot;
using System;
using System.IO;

[Tool]
public partial class InfoLabel : Label3D
{
    public void _func_godot_apply_properties(Godot.Collections.Dictionary<string, Variant> entity_properties)
    {
        if (entity_properties.ContainsKey("text"))
        {
            Text = entity_properties["text"].AsString();
        }
    }
}
