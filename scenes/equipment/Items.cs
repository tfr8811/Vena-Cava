using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Items : Node3D
{
    [Export]
    private PackedScene[] inventory;
    private Gun currentGun;
    private int currentIndex = 0;
    public override void _Ready()
    {
        currentGun = (Gun)inventory[currentIndex].Instantiate();
        AddChild(currentGun);
    }
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ScrollUp"))
        {
            if (currentIndex < inventory.Length-1)
            {
                currentIndex++;
                RemoveChild(currentGun);
                currentGun = (Gun)inventory[currentIndex].Instantiate();
                AddChild(currentGun);
            } else
            {
                currentIndex = 0;
                RemoveChild(currentGun);
                currentGun = (Gun)inventory[0].Instantiate();
                AddChild(currentGun);
            }
        }
        if (Input.IsActionJustPressed("ScrollDown"))
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                RemoveChild(currentGun);
                currentGun = (Gun)inventory[currentIndex].Instantiate();
                AddChild(currentGun);
            } else
            {
                currentIndex = inventory.Length - 1;
                RemoveChild(currentGun);
                currentGun = (Gun)inventory[currentIndex].Instantiate();
                AddChild(currentGun);
            }
        }
    }
}
