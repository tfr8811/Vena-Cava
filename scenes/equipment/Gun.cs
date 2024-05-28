using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 5/09/2024
/// Controls the behavior of the players guns
/// </summary>
public partial class Gun : Node3D
{
    // shooting
    const float BULLET_SPEED = 120.0f;

    // ammo
    private int maxAmmo = 10;
    private int ammo = 10;

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

    // todo: use GD.Timer
    private double maxFireDelay = 0.2;
    private double fireDelay;

    // instance the bullet
    PackedScene psBullet = GD.Load<PackedScene>("res://scenes/equipment/Bullet.tscn");

    public override void _Ready()
    {
        gunSprite.Play("Idle");
        equipSound.Play();

        // setup the ammoCounter
        ammoCounter.Text = ammo.ToString();
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
        if (canShoot && ammo > 0 && fireDelay <= 0 && Input.IsActionJustPressed("Shoot"))
        {
            gunSprite.Play("Shoot");
            shootSound.Play();
            SpawnBullet(BULLET_SPEED);

            // Here I can use can shoot or fire delay, for now, I'll use can shoot
            canShoot = false;
            // fireDelay = maxFireDelay;

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
        if ((canShoot && ammo == 0) || Input.IsActionJustPressed("Reload"))
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
    private void SpawnBullet(float speed)
    {
        Bullet bullet = (Bullet)psBullet.Instantiate();
        GetNode("/root/World").AddChild(bullet);
        // set the position of the bullet in front of the player
        Vector3 pointVector = -this.GlobalTransform.Basis.Z;
        bullet.GlobalPosition = this.GlobalPosition;
        bullet.GlobalPosition += pointVector * 1f;
        // set the collision mask of the bullet to the enemy layer (3)
        bullet.SetCollisionMaskValue(3, true);

        bullet.Velocity = pointVector * speed;
    }
}
