using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 5/09/2024
/// Controls the behavior of the players guns
/// </summary>
public partial class Gun : Node3D, IEquippable
{
    // Gun parameters
    [Export]
    private float bulletSpeed; // recommended: 120.0f, worth speeding up for rifle
    [Export]
    private int maxAmmo;
    private int ammo;
    [Export]
    private int bulletDamage;
    [Export]
    private int projectileCount;
    [Export]
    private float projectileSpread;
    [Export]
    private double maxBulletLifespan;
    [Export]
    private double maxFireDelay; // pistol's is 0.2, for reference
    private double fireDelay;
    [Export]
    private bool autofire;

    // Gun info - TODO should refactor some of the player code to the gun
    [Export]
    private AnimatedSprite2D gunSprite;
    [Export]
    private Label ammoCounter;
    [Export]
    private AudioStreamPlayer3D shootSound;
    [Export]
    private AudioStreamPlayer3D reloadSound;
    [Export]
    private AudioStreamPlayer3D equipSound;

    // the player cant shoot during some actions such as reloading
    private bool canShoot = true;

    RandomNumberGenerator rng;

    // instance the bullet
    PackedScene psBullet = GD.Load<PackedScene>("res://scenes/equipment/Bullet.tscn");

    public override void _Ready()
    {
        gunSprite.Play("Idle");
        equipSound.Play();
        ammo = maxAmmo;

        // setup the ammoCounter
        ammoCounter.Text = ammo.ToString();

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
        if (canShoot && ammo > 0 && fireDelay <= 0 
            && ( Input.IsActionJustPressed("Shoot") || (Input.IsActionPressed("Shoot") && autofire)) )
        {
            gunSprite.Play("Shoot");
            shootSound.Play();
            SpawnBullet(bulletSpeed, bulletDamage, projectileCount, projectileSpread, maxBulletLifespan);

            // Here I can use can shoot or fire delay, for now, I'll use fireDelay
            fireDelay = maxFireDelay;

            // update the ammo
            ammo -= 1;
            ammoCounter.Text = ammo.ToString();
        }

        // shoot animation handler
        if (gunSprite.Animation == "Shoot")
        {
            if (!gunSprite.IsPlaying())
            {
                gunSprite.Play("Idle");
                canShoot = true;
            }
        }

        // RELOAD - It's automatic rn but can also be done by pressing "R"
        if (canShoot && fireDelay <= 0 && (ammo == 0 || (Input.IsActionJustPressed("Reload") && ammo < maxAmmo)))
        {
            gunSprite.Play("Reload");
            reloadSound.Play();
            canShoot = false;
        }
        // reload animation handler
        if (gunSprite.Animation == "Reload")
        {
            if (!gunSprite.IsPlaying())
            {
                ammo = maxAmmo;
                ammoCounter.Text = ammo.ToString();
                gunSprite.Play("Idle");
                canShoot = true;
            }
        }
    }

    /// <summary>
    /// Roman Noodles
    /// 4/30/2024
    /// Shoots a bullet in the direction the player is facing
    /// </summary>
    private void SpawnBullet(float speed, int damage, int count, float spread, double lifespan)
    {
        for (int i = 0; i < count; i++) {
            Bullet bullet = (Bullet)psBullet.Instantiate();
            GetNode("/root/World").AddChild(bullet);
            // calculate the bullets trajectory
            Vector3 pointVector = -this.GlobalTransform.Basis.Z;
            bullet.GlobalPosition = this.GlobalPosition;
            // set the collision mask of the bullet to the enemy layer (3)
            bullet.SetCollisionMaskValue(3, true);
            // guns that shoot in bursts are stronger close up
            bullet.BulletTimer = lifespan * (((float) i + 1f) / (float) count);
            bullet.Damage = damage;
            // apply spread
            Vector3 vSpread = new Vector3(rng.Randf() - 0.5f, rng.Randf() - 0.5f, rng.Randf()-0.5f);
            vSpread = vSpread.Normalized() * spread;
            bullet.Velocity = (pointVector * speed) + vSpread;
        }
    }

    /// <summary>
    /// Roman Noodles
    /// 5/29/2024
    /// Handles the guns behavior when equipped
    /// </summary>
    public void Equip()
    {
        gunSprite.Play("Idle");
        if (shootSound.Playing)
        {
            shootSound.Stop();
        }
        if (reloadSound.Playing)
        {
            reloadSound.Stop();
        }
        equipSound.Play();
        canShoot = true;
    }
}
