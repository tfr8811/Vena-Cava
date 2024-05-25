using Godot;
using System;

public partial class Hud : CanvasLayer
{
    private TextureRect crosshair;
    private TextureRect hand;
    private ColorRect damageFlash;
    private Status status;

    private float hitOpacity = 0.33f;

    public override void _Ready()
    {
        crosshair = GetNode<TextureRect>("Hud/Crosshair");
        hand = GetNode<TextureRect>("Hud/Hand");
        damageFlash = GetNode<ColorRect>("Hud/DamageFlash");
        status = GetNode<Status>("Hud/Hud/Status");
    }

    public void ShowCrosshair()
    {
        hand.Hide();
        crosshair.Show();
    }

    public void ShowHand()
    {
        crosshair.Hide();
        hand.Show();
    }

    public void DamageFlash()
    {
        damageFlash.Color = new Color(damageFlash.Color.R, damageFlash.Color.G, damageFlash.Color.B, hitOpacity);
    }

    public void Update(Player player)
    {
        
    }

    public override void _Process(double delta)
    {
        if (damageFlash.Color.A > 0)
        {
            damageFlash.Color = new Color(damageFlash.Color.R, damageFlash.Color.G, damageFlash.Color.B, damageFlash.Color.A - (float)delta);
        }
    }
}
