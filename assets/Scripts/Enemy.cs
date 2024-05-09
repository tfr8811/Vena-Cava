using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 5/01/2024
/// Controls and Manages the cop enemy
/// </summary>

public partial class Enemy : CharacterBody3D, IDamageable
{
    // player reference
    private CharacterBody3D player = null;

    [Export]
    private NodePath playerPath;

    [Export]
    private AnimatedSprite3D enemySprite;

    // health
    private int health = 3;

    // navigation agent reference
    private NavigationAgent3D navAgent;

    [Export]
    private NodePath navPath;


    // movement
    const float SPEED = 6f;

    // shooting
    const float BULLET_SPEED = 120.0f;
    private bool canShoot = true;
    // enemy cannot move while shooting or reloading
    private bool canMove = true;

    // AI stuff
    private float detectionRadius = 115.0f;
    private bool hasSeenPlayer = false;
    private float standOffRadius = 5.0f;
    private float shootRadius = 10.0f;
    RayCast3D sightRaycast;
    [Export]
    private NodePath sightPath;

    private double maxFireDelay = 1;
    private double fireDelay;
    private int maxBulletCount = 8;
    private int bulletCount = 8;

    // Sounds
    [Export]
    private AudioStreamPlayer3D commandRandomizer;
    [Export]
    private AudioStreamPlayer3D damageRandomizer;
    [Export]
    private AudioStreamPlayer3D shootSound;
    [Export]
    private AudioStreamPlayer3D reloadSound;

    // instance the bullet
    PackedScene psBullet = GD.Load<PackedScene>("res://assets/Scenes/Bullet.tscn");

    public override void _Ready()
    {
        player = (CharacterBody3D)GetNode(playerPath);
        navAgent = (NavigationAgent3D)GetNode(navPath);
        sightRaycast = (RayCast3D)GetNode(sightPath);
        fireDelay = maxFireDelay;
        enemySprite.Play("Idle");
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

        // reset velocity
        Velocity = Vector3.Zero;

        // navigate to player
        if (IsInstanceValid(player))
        {
            // FACE PLAYER - enemy always faces player as the sprites are 2d
            LookAt(new Vector3(player.GlobalPosition.X, GlobalPosition.Y, player.GlobalPosition.Z), Vector3.Up);
            
            // MOVE
            Vector3 playerRelativePosition = player.GlobalPosition - this.GlobalPosition;
            // chases player when they are in site or if they have already spotted the player
            if (canMove && (hasSeenPlayer || CheckCanSeePlayer(playerRelativePosition)))
            {
                if (standOffRadius < playerRelativePosition.Length())
                {
                    // The enemy is moving:
                    enemySprite.Play("Run");

                    // this enemy has noticed the player, and therefore, their detection radius should increase due to vigilance
                    if (!hasSeenPlayer)
                    {
                        commandRandomizer.Play();
                        hasSeenPlayer = true;
                    }
                    navAgent.TargetPosition = player.GlobalTransform.Origin;
                    Vector3 nextNavPoint = navAgent.GetNextPathPosition();
                    Velocity = (nextNavPoint - GlobalTransform.Origin).Normalized() * SPEED;
                } else
                {
                    // The enemy is not moving:
                    enemySprite.Play("Idle");
                }
            }

            // SHOOT
            // fires gun when player is in sight and close enough to shoot
            if (canShoot && shootRadius > playerRelativePosition.Length() && CheckCanSeePlayer(playerRelativePosition))
            {
                if (bulletCount > 0)
                {
                    if (fireDelay <= 0)
                    {
                        // SHOOT THE BULLET
                        enemySprite.Play("Shoot");
                        shootSound.Play();
                        bulletCount -= 1;
                        SpawnBullet(BULLET_SPEED);
                        canShoot = false;
                        canMove = false;
                    }
                }
                else
                {
                    // start the reload delay and reload the gun
                    enemySprite.Play("Reload");
                    reloadSound.Play();
                    canShoot = false;
                    canMove = false;
                }
            }
            // shoot animation handler
            if (enemySprite.Animation == "Shoot")
            {
                if (!enemySprite.IsPlaying())
                {
                    enemySprite.Play("Idle");
                    canShoot = true;
                    canMove = true;
                }
            }
            // reload animation handler
            if (enemySprite.Animation == "Reload")
            {
                if (!enemySprite.IsPlaying())
                {
                    bulletCount = maxBulletCount;
                    enemySprite.Play("Idle");
                    canShoot = true;
                    canMove = true;

                    // right now I'm applying fire delay after reload so the enemy at some point tries to get closer to the player
                    fireDelay = maxFireDelay;
                }
            }

            // death animation handler
            if (enemySprite.Animation == "Death")
            {
                if (!enemySprite.IsPlaying())
                {
                    // despawn the enemy
                    this.QueueFree();
                }
            }
        }

        // move if the enemy is not doing an action that prevents them from moving
        if (canMove) {
            MoveAndSlide();
        }
    }

    /// <summary>
    /// Roman Noodles
    /// 5/01/2024
    /// Shoots a bullet in the direction the enemy is facing
    /// </summary>
    public void SpawnBullet(float speed)
    {
        Bullet bullet = (Bullet)psBullet.Instantiate();
        GetNode("/root").AddChild(bullet);
        // set the position of the bullet in front of the player
        Vector3 pointVector = -GlobalTransform.Basis.Z;
        bullet.GlobalPosition = GlobalPosition + new Vector3(0, 0.5f, 0);
        bullet.GlobalPosition += pointVector * 2;


        bullet.Velocity = pointVector * speed;
    }

    /// <summary>
    /// Roman Noodles (Adapted from Impulse)
    /// 5/01/2024
    /// Checks if the player is in the line of sight of the enemy
    /// </summary>
    public bool CheckCanSeePlayer(Vector3 relativeTargetVector)
    {
        bool inRadius = relativeTargetVector != Vector3.Zero && relativeTargetVector.Length() <= detectionRadius;
        if (inRadius)
        {
            sightRaycast.Rotation = -this.GlobalRotation;
            sightRaycast.TargetPosition = relativeTargetVector;
            bool lineOfSight = sightRaycast.GetCollider() == player;
            return lineOfSight;
        }
        return false;
    }

    /// <summary>
    /// Roman Noodles
    /// 2/6/2024
    /// Damages the enemy by the amount specified
    /// </summary>
    public void TakeDamage(int damage)
    {
        damageRandomizer.Play();
        health -= damage;
        if (health <= 0) enemySprite.Play("Death");
    }
}
