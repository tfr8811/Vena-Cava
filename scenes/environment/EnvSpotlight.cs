using Godot;
using System;
using System.Diagnostics;

[Tool]
public partial class EnvSpotlight : SpotLight3D, IToggleable
{

    private bool Enabled = true;

    public void Enable()
    {
        Enabled = true;
        SetEnergy();
    }

    public void Disable()
    {
        Enabled = false;
        SetEnergy();
    }

    public void Toggle()
    {
        Enabled = !Enabled;
        SetEnergy();
    }

    private void SetEnergy() {
        if (Enabled)
        {
            LightEnergy = 5;
        } else
        {
            LightEnergy = 0;
        }
    }
}
