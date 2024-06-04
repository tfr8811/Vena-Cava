using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 6/03/2024
/// Controls and Manages the hobo allies
/// </summary>

public partial class Hobo : CharacterBody3D, IDamageable
{
    // ANIMATIONS
    [Export]
    private AnimatedSprite3D hoboFront;
    [Export]
    private AnimatedSprite3D hoboSide;
    [Export]
    private AnimatedSprite3D hoboRear;


    // navigation agent reference
    private NavigationAgent3D navAgent;
    [Export]
    private NodePath navPath;

    // health
    [Export]
    private int health;

    // movement
    [Export]
    private float speed;

    // shooting
    // Gun parameters
    [Export]
    private float bulletSpeed; // recommended: 120.0f, worth speeding up for rifle
    [Export]
    private float shotHeight;
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
    private double maxFireDelay;
    private double fireDelay;
    [Export]
    private double maxPostReloadDelay;
    private double postReloadDelay;

    private bool canShoot = true;
    // enemy cannot move while shooting or reloading
    private bool canMove = true;

    // AI stuff
    private float detectionRadius = 115.0f;
    [Export]
    private float standOffRadius;
    [Export]
    private float shootRadius;
    RayCast3D sightRaycast;
    [Export]
    private NodePath sightPath;

    // Sounds
    [Export]
    private AudioStreamPlayer3D commandRandomizer;
    [Export]
    private AudioStreamPlayer3D damageRandomizer;
    [Export]
    private AudioStreamPlayer3D calloutRandomizer;
    [Export]
    private AudioStreamPlayer3D shootSound;
    [Export]
    private AudioStreamPlayer3D reloadSound;

    // instance the bullet
    PackedScene psBullet = GD.Load<PackedScene>("res://scenes/equipment/Bullet.tscn");

    Enemy currentFightTarget;
    public bool fighting = false;
    public bool recruited = false;

    private float facingAngle = 0.0f;

    RandomNumberGenerator rng;

    public override void _Ready()
    {
        navAgent = (NavigationAgent3D)GetNode(navPath);
        sightRaycast = (RayCast3D)GetNode(sightPath);
        fireDelay = maxFireDelay;
        postReloadDelay = maxPostReloadDelay;
        PlayAnimation("Idle");
        ammo = maxAmmo;

        rng = new RandomNumberGenerator();
    }

    public void PlayAnimation(StringName name)
    {
        hoboFront.Play(name);
        hoboSide.Play(name);
        hoboRear.Play(name);
    }

    public void FollowPlayer(Player player)
    {
        // MOVE
        Vector2 targetRelativePosition = new Vector2(player.GlobalPosition.X, player.GlobalPosition.Z) - new Vector2(GlobalPosition.X, GlobalPosition.Z);
        if (canMove)
        {
            if (2 < targetRelativePosition.Length())
            {
                // The enemy is moving:
                PlayAnimation("Run");
                // navigate behind the player
                navAgent.TargetPosition = player.GlobalTransform.Origin + player.Head.GlobalTransform.Basis.Z * 2;
                Vector3 nextNavPoint = navAgent.GetNextPathPosition();
                Velocity = (nextNavPoint - GlobalTransform.Origin).Normalized() * speed;
            }
            else
            {
                // The enemy is not moving:
                PlayAnimation("Idle");
            }
        }
    }

    public void FightTarget(Enemy target, Player player)
    {
        // navigate to enemy
        if (IsInstanceValid(target))
        {
            // MOVE
            Vector3 targetRelativePosition = target.GlobalPosition - this.GlobalPosition;
            if (canMove)
            {
                if (standOffRadius < targetRelativePosition.Length())
                {
                    // The enemy is moving:
                    PlayAnimation("Run");

                    navAgent.TargetPosition = target.GlobalTransform.Origin;
                    Vector3 nextNavPoint = navAgent.GetNextPathPosition();
                    Velocity = (nextNavPoint - GlobalTransform.Origin).Normalized() * speed;
                }
                else
                {
                    // The enemy is not moving:
                    PlayAnimation("Idle");
                }
            }
            // SHOOT
            // fires gun when target is in sight and close enough to shoot
            if (canShoot && shootRadius > targetRelativePosition.Length() && CheckCanSeeTarget(targetRelativePosition, target))
            {
                facingAngle = (new Vector2(targetRelativePosition.X, targetRelativePosition.Z)).Angle();
                if (ammo > 0)
                {
                    if (postReloadDelay <= 0 && fireDelay <= 0)
                    {
                        // SHOOT THE BULLET
                        PlayAnimation("Shoot");
                        shootSound.Play();
                        ammo -= 1;
                        SpawnBullet(target, bulletSpeed, bulletDamage, projectileCount, projectileSpread, maxBulletLifespan);
                        canShoot = false;
                        canMove = false;
                        fireDelay = maxFireDelay;
                    }
                }
                else
                {
                    // start the reload delay and reload the gun
                    PlayAnimation("Reload");
                    reloadSound.Play();
                    canShoot = false;
                    canMove = false;
                }
            }
        } else
        {
            fighting = false;
        }
    }

    public override void _Process(double delta)
    {
        // decrease delay
        if (fireDelay > 0)
        {
            fireDelay -= delta;
        }
        if (postReloadDelay > 0)
        {
            postReloadDelay -= delta;
        }
        // shoot animation handler
        if (hoboFront.Animation == "Shoot")
        {
            if (!hoboFront.IsPlaying() && fireDelay <= 0)
            {
                PlayAnimation("Idle");
                canShoot = true;
                canMove = true;
            }
        }
        // reload animation handler
        if (hoboFront.Animation == "Reload")
        {
            if (!hoboFront.IsPlaying())
            {
                ammo = maxAmmo;
                PlayAnimation("Idle");
                canShoot = true;
                canMove = true;

                // postReloadDelay exists so the enemy at some point tries to get closer to the player
                postReloadDelay = maxPostReloadDelay;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // reset velocity
        Velocity = Vector3.Zero;

        Player player = GlobalWorldState.Instance.Player;

        // handle movement
        if (IsInstanceValid(player))
        {
            // FACE PLAYER - enemy always faces player as the sprites are 2d
            LookAt(new Vector3(player.GlobalPosition.X, GlobalPosition.Y, player.GlobalPosition.Z), Vector3.Up);
            if (fighting)
            {
                FightTarget(currentFightTarget, player);
            } else if (recruited)
            {
                FollowPlayer(player);
            }
        }

        // for testing
        //Velocity = new Vector3(1, 0, 0);

        // move if the hobo is not doing an action that prevents them from moving
        if (canMove)
        {
            MoveAndSlide();
        }
        // animation direction handler
        if (Velocity.Length() > 0)
        { 
            facingAngle = (new Vector2(Velocity.X, Velocity.Z)).Angle(); 
        }
        Vector2 relativeDirectionToPlayer2D = new Vector2(player.GlobalPosition.X - this.GlobalPosition.X,
                                                            player.GlobalPosition.Z - this.GlobalPosition.Z);
        AnimationUtil.Direction dir = AnimationUtil.GetDirection(relativeDirectionToPlayer2D.Angle(), facingAngle);
        switch (dir)
        {
            case AnimationUtil.Direction.AWAY:
                hoboFront.Hide();
                hoboSide.Hide();
                hoboRear.Show();
                break;
            case AnimationUtil.Direction.RIGHT:
                hoboFront.Hide();
                hoboSide.Show();
                hoboRear.Hide();
                hoboSide.FlipH = false;
                break;
            case AnimationUtil.Direction.LEFT:
                hoboFront.Hide();
                hoboSide.Show();
                hoboRear.Hide();
                hoboSide.FlipH = true;
                break;
            case AnimationUtil.Direction.TOWARDS:
                hoboFront.Show();
                hoboSide.Hide();
                hoboRear.Hide();
                break;
        }
    }

    /// <summary>
    /// Roman Noodles
    /// 5/01/2024
    /// Shoots a bullet in the direction the hobo is facing
    /// </summary>
    private void SpawnBullet(Enemy target, float bSpeed, int damage, int count, float spread, double lifespan)
    {
        for (int i = 0; i < count; i++)
        {
            Bullet bullet = (Bullet)psBullet.Instantiate();
            GetNode("/root/World").AddChild(bullet);

            // calculate the bullets trajectory
            Vector3 pointVector = new Vector3(
                target.GlobalPosition.X - GlobalPosition.X,
                target.GlobalPosition.Y - (GlobalPosition.Y + shotHeight),
                target.GlobalPosition.Z - GlobalPosition.Z
                );
            pointVector = pointVector.Normalized();
            bullet.GlobalPosition = GlobalPosition + new Vector3(0, shotHeight, 0);
            // set the collision mask of the bullet to the enemy layer (3)
            bullet.SetCollisionMaskValue(3, true);
            // guns that shoot in bursts are stronger close up
            bullet.BulletTimer = lifespan * (((float)i + 1f) / (float)count);
            bullet.Damage = damage;
            // apply spread
            if (spread > 0)
            {
                Vector3 vSpread = new Vector3(rng.Randf() - 0.5f, rng.Randf() - 0.5f, rng.Randf() - 0.5f);
                vSpread = vSpread.Normalized() * spread;
                bullet.Velocity = (pointVector * bSpeed) + vSpread;
            }
            else
            {
                bullet.Velocity = (pointVector * bSpeed);
            }
        }
    }

    /// <summary>
    /// Roman Noodles (Adapted from Impulse)
    /// 5/01/2024
    /// Checks if the target is in the line of sight of the hobo
    /// </summary>
    public bool CheckCanSeeTarget(Vector3 relativeTargetVector, Node3D target)
    {
        bool inRadius = relativeTargetVector != Vector3.Zero && relativeTargetVector.Length() <= detectionRadius;
        if (inRadius)
        {
            sightRaycast.Rotation = -this.GlobalRotation;
            sightRaycast.TargetPosition = relativeTargetVector;
            bool lineOfSight = sightRaycast.GetCollider() == target;
            return lineOfSight;
        }
        return false;
    }

    /// <summary>
    /// Roman Noodles
    /// 2/6/2024
    /// Damages the hobo by the amount specified
    /// </summary>
    public void TakeDamage(int damage)
    {
        //damageRandomizer.Play();
        health -= damage;
        if (health <= 0)
        {
            PlayAnimation("Death");
            canMove = false;
            canShoot = false;
        }

    }

    public void SetTarget(Enemy target)
    {
        currentFightTarget = target;
        fighting = true;
    }
}
