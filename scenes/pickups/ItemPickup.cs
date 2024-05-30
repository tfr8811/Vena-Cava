using Godot;
using System;

public partial class ItemPickup : Node3D
{
    [Export]
    private string name;
    [Export]
    private PackedScene psPickup;
    public void _on_area_3d_body_entered(Node3D body)
    {
        if (body is Player)
        {
            // get the players Items node
            Items items = (Items) ((Player)body).FindChild("Items");
            if (!items.hasItem(name))
            {
                items.AddItem(psPickup);
                this.QueueFree();
            }
        }
    }
}
