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
        // preload all of the player's items
        if (inventory.Length > 0)
        {
            loadedItems = new Node[inventory.Length];
            for (int i = 0; i < inventory.Length; i++)
            {
                loadedItems[i] = inventory[i].Instantiate();
            }
            AddChild(loadedItems[currentIndex]);
        }
    }
    public override void _Process(double delta)
    {
        if (inventory.Length > 0)
        {
            if (Input.IsActionJustPressed("ScrollUp"))
            {
                if (currentIndex < inventory.Length - 1)
                {
                    SwapToIndex(currentIndex + 1);
                }
                else
                {
                    SwapToIndex(0);
                }
            }
            if (Input.IsActionJustPressed("ScrollDown"))
            {
                if (currentIndex > 0)
                {
                    SwapToIndex(currentIndex - 1);
                }
                else
                {
                    SwapToIndex(inventory.Length - 1);
                }
            }
        }
    }
    /// <summary>
    /// Roman Noodles
    /// 5/29/2024
    /// Adds an item to the player's inventory
    /// </summary>
    public void AddItem(PackedScene item)
    {
        // add the new item to the arrays
        PackedScene[] newInventory = new PackedScene[inventory.Length + 1];
        Node[] newLoadedItems = new Node[loadedItems.Length + 1];
        for (int i = 0; i < inventory.Length; i++)
        {
            newInventory[i] = inventory[i];
            newLoadedItems[i] = loadedItems[i];
        }
        newInventory[inventory.Length] = item;
        newLoadedItems[loadedItems.Length] = item.Instantiate();
        inventory = newInventory;
        loadedItems = newLoadedItems;
        // swap to the new item
        SwapToIndex(inventory.Length-1);
    }

    /// <summary>
    /// Roman Noodles
    /// 5/29/2024
    /// swaps from one item to another
    /// </summary>
    /// <param name="index">index of the item to swap to</param>
    private void SwapToIndex(int index)
    {
        RemoveChild(loadedItems[currentIndex]);
        currentIndex = index;
        AddChild(loadedItems[currentIndex]);
        if (loadedItems[currentIndex] is IEquippable)
        {
            ((IEquippable)loadedItems[currentIndex]).Equip();
        }
    }

    public bool hasItem(string name)
    {
        for (int i = 0; i < loadedItems.Length; i++)
        {
            if (loadedItems[i].Name == name)
            {
                return true;
            }
        }
        return false;
    }
}
