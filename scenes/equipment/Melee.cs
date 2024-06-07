using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 5/09/2024
/// Controls the behavior of the players melee weapons
/// </summary>
public partial class Melee : Node3D, IEquippable
{
    // Weapon parameters
    [Export]
    private int damage;
    [Export]
    private int hitFrame;
    [Export]
    private double maxFireDelay; // pistol's is 0.2, for reference
    private double fireDelay;
    [Export]
    private bool autofire;

    // Gun info - TODO should refactor some of the player code to the gun
    [Export]
    private AnimatedSprite2D weaponSprite;
    [Export]
    private AudioStreamPlayer3D hitSound;
    [Export]
    private AudioStreamPlayer3D equipSound;

    // the player cant shoot during some actions such as reloading
    private bool canShoot = true;

    bool hitTarget = false;

    RandomNumberGenerator rng;

    // hitscan
    [Export]
    RayCast3D hitScan;

    public override void _Ready()
    {
        weaponSprite.Play("Idle");
        equipSound.Play();

        rng = new RandomNumberGenerator();
    }

    public override void _Process(double delta)
    {
        // decrease delay
        if (fireDelay > 0)
        {
            fireDelay -= delta;
        }

    }

    public override void _PhysicsProcess(double delta)
    {
        // SHOOT
        if (canShoot && fireDelay <= 0
            && (Input.IsActionJustPressed("Shoot") || (Input.IsActionPressed("Shoot") && autofire)))
        {
            weaponSprite.Play("Shoot");

            // Here I can use can shoot or fire delay, for now, I'll use fireDelay
            fireDelay = maxFireDelay;
        }

        // shoot animation handler
        if (weaponSprite.Animation == "Shoot")
        {
            if (!hitTarget && weaponSprite.Frame == hitFrame && hitScan.IsColliding())
            {
                hitTarget = true;
                hitSound.Play();
                Object collider = hitScan.GetCollider();

                if (collider is IDamageable)
                {
                    ((IDamageable)collider).TakeDamage(damage);
                }
            }
            if (!weaponSprite.IsPlaying())
            {
                weaponSprite.Play("Idle");
                canShoot = true;
                hitTarget = false;
            }
        }
    }

    /// <summary>
    /// Roman Noodles
    /// 5/29/2024
    /// Handles the guns behavior when equipped
    /// </summary>
    public void Equip()
    {
        weaponSprite.Play("Idle");
        if (hitSound.Playing)
        {
            hitSound.Stop();
        }
        equipSound.Play();
        canShoot = true;
    }
}
