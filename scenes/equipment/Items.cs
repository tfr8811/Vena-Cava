using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Items : Node3D
{
    [Export]
    private PackedScene[] inventory;
    private Node[] loadedItems;
    private int currentIndex = 0;
    public override void _Ready()
    {
        // I'll need to resize this when player picks up an item
        loadedItems = new Node[inventory.Length];
        for (int i = 0; i < inventory.Length; i++)
        {
            loadedItems[i] = inventory[i].Instantiate();
        }
        AddChild(loadedItems[currentIndex]);
    }
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ScrollUp"))
        {
            if (currentIndex < inventory.Length-1)
            {
                RemoveChild(loadedItems[currentIndex]);
                currentIndex++;
                AddChild(loadedItems[currentIndex]);
            } else
            {
                RemoveChild(loadedItems[currentIndex]);
                currentIndex = 0;
                AddChild(loadedItems[currentIndex]);
            }
        }
        if (Input.IsActionJustPressed("ScrollDown"))
        {
            if (currentIndex > 0)
            {
                RemoveChild(loadedItems[currentIndex]);
                currentIndex--;
                AddChild(loadedItems[currentIndex]);
            } else
            {
                RemoveChild(loadedItems[currentIndex]);
                currentIndex = inventory.Length - 1;
                AddChild(loadedItems[currentIndex]);
            }
        }
    }
}
