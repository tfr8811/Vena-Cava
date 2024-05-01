using Godot;
using System;

public partial class Enemy : CharacterBody3D, IDamageable
{
    // player reference
    private CharacterBody3D player = null;

    [Export]
    private NodePath playerPath;

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

    // AI stuff
    private float detectionRadius = 115.0f;
    private bool hasSeenPlayer = false;
    private float standOffRadius = 5.0f;
    RayCast3D sightRaycast;
    [Export]
    private NodePath sightPath;

    private double maxFireDelay = 0.5;
    private double fireDelay;
    private double maxReloadDelay = 3.0;
    private double releodDelay;
    private int maxBulletCount = 8;
    private int bulletCount = 8;

    // instance the bullet - Tom
    PackedScene psBullet = GD.Load<PackedScene>("res://assets/Scenes/Bullet.tscn");

    public override void _Ready()
    {
        player = (CharacterBody3D)GetNode(playerPath);
        navAgent = (NavigationAgent3D)GetNode(navPath);
        sightRaycast = (RayCast3D)GetNode(sightPath);
        fireDelay = maxFireDelay;
    }

    public override void _Process(double delta)
    {
        // decrease delay
        if (fireDelay > 0)
        {
            fireDelay -= delta;
        }
        if (releodDelay > 0)
        {
            releodDelay -= delta;
        }
    }

    public override void _PhysicsProcess(double delta)
    {

        // reset velocity
        Velocity = Vector3.Zero;

        // navigate to player
        if (IsInstanceValid(player))
        {
            Vector3 playerRelativePosition = player.GlobalPosition - this.GlobalPosition;
            // chases player when they are in site or if they have already spotted the player
            if (hasSeenPlayer || CheckCanSeePlayer(playerRelativePosition))
            {
                if (standOffRadius < playerRelativePosition.Length())
                {
                    // this enemy has noticed the player, and therefore, their detection radius should increase due to vigilance
                    if (!hasSeenPlayer)
                    {
                        hasSeenPlayer = true;
                    }
                    navAgent.TargetPosition = player.GlobalTransform.Origin;
                    Vector3 nextNavPoint = navAgent.GetNextPathPosition();
                    Velocity = (nextNavPoint - GlobalTransform.Origin).Normalized() * SPEED;
                }

                LookAt(new Vector3(player.GlobalPosition.X, GlobalPosition.Y, player.GlobalPosition.Z), Vector3.Up);
            }

            // fires gun when player is in sight
            if (CheckCanSeePlayer(playerRelativePosition))
            {
                if (bulletCount > 0)
                {
                    if (fireDelay <= 0 && releodDelay <= 0)
                    {
                        bulletCount -= 1;
                        SpawnBullet(BULLET_SPEED);
                        fireDelay = maxFireDelay;
                    }
                }
                else
                {
                    // start the reload delay and reload the gun
                    releodDelay = maxReloadDelay;
                    bulletCount = maxBulletCount;
                }
            }
        }

        MoveAndSlide();
    }

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
        health -= damage;
        if (health <= 0) this.QueueFree();
    }
}
