using Godot;
using System;

public partial class GlobalWorldState : Node
{
    public static GlobalWorldState Instance { get { return instance; } }
    private static GlobalWorldState instance;
    public Player Player { get; set; }
    public Node3D WorldRoot { get; set; }

    public override void _Ready()
    {
        instance = this;
    }
}
