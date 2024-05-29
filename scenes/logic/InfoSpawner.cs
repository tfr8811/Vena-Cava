using Godot;

[Tool]
public partial class InfoSpawner : Node3D, ITriggerable
{

	[Export]
	private PackedScene _node;

    public void Trigger(Node activator)
    {
        Node3D spawned = _node.Instantiate<Node3D>();

        GetParent().AddChild(spawned);
        spawned.Rotation = Rotation;
        spawned.Position = Position;
    }

    public void _func_godot_apply_properties(Godot.Collections.Dictionary<string, Variant> entity_properties)
    {
        if (entity_properties.ContainsKey("path"))
        {
            string path = entity_properties["path"].AsString();
            if (string.IsNullOrEmpty(path))
            {
                GD.PushError(string.Format("Spawner at: {} has no entity!", entity_properties["origin"].AsVector3()));
            }

            if (!ResourceLoader.Exists(path))
            {
                GD.PushError(string.Format("Spawner at: {} has invalid entity \"{}\"!", entity_properties["origin"].AsVector3()));
                return;
            }

            _node = ResourceLoader.Load<PackedScene>(path);
        }
    }
}
