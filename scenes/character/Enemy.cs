using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 5/01/2024
/// Controls and Manages the enemies
/// </summary>

public partial class Enemy : CharacterBody3D, IDamageable
{
    // ANIMATIONS
    [Export]
    private AnimatedSprite3D enemyFront;
    [Export]
    private AnimatedSprite3D enemySide;
    [Export]
    private AnimatedSprite3D enemyRear;

    // navigation agent reference
    private NavigationAgent3D navAgent;
    [Export]
    private NodePath navPath;
    [Export]
    private Area3D nearbyDetection;

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
	private bool hasSeenPlayer = false;
	public bool HasSeenPlayer { 
		get { 
			return hasSeenPlayer; 
		} 
		set {
			hasSeenPlayer = value;
		}
	}
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

    CharacterBody3D currentFightTarget;
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
        enemyFront.Play(name);
        enemySide.Play(name);
        enemyRear.Play(name);
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
        if (enemyFront.Animation == "Shoot")
        {
            if (!enemyFront.IsPlaying() && fireDelay <= 0)
            {
                PlayAnimation("Idle");
                canShoot = true;
                canMove = true;
            }
        }
        // reload animation handler
        if (enemyFront.Animation == "Reload")
        {
            if (!enemyFront.IsPlaying())
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

    public void FightTarget(CharacterBody3D target)
	{
		// navigate to player
		if (IsInstanceValid(target))
		{
			// MOVE
			Vector3 targetRelativePosition = target.GlobalPosition - this.GlobalPosition;
			// chases player when they are in site or if they have already spotted the player
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
			// fires gun when player is in sight and close enough to shoot
			if (canShoot && shootRadius > targetRelativePosition.Length() && CheckCanSeeTarget(target))
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
		}
        else
        {
            fighting = false;
        }
    }

	public override void _PhysicsProcess(double delta)
	{

		// reset velocity
		Velocity = Vector3.Zero;

		Player player = GlobalWorldState.Instance.Player;

        // the cop will look for the closest thing to target
        CheckSurroundings();

        // handle movement
        if (IsInstanceValid(player))
        {
            // FACE PLAYER - enemy always faces player as the sprites are 2d
            LookAt(new Vector3(player.GlobalPosition.X, GlobalPosition.Y, player.GlobalPosition.Z), Vector3.Up);
            if (fighting)
            {
                FightTarget(currentFightTarget);
            } else if (CheckCanSeeTarget(player))
            {
                SetTarget(player);
            }
        }

        // move if the enemy is not doing an action that prevents them from moving
        if (canMove) {
			MoveAndSlide();
		}

        // death animation handler
        if (enemyFront.Animation == "Death")
        {
            if (!enemyFront.IsPlaying())
            {
                // tell the gamemanager that this enemy was defeated
                player.EmitSignal("enemyDefeated");
                // despawn the enemy
                this.QueueFree();
            }
        }

        // HANDLE ANIMATION
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
                enemyFront.Hide();
                enemySide.Hide();
                enemyRear.Show();
                break;
            case AnimationUtil.Direction.RIGHT:
                enemyFront.Hide();
                enemySide.Show();
                enemyRear.Hide();
                enemySide.FlipH = false;
                break;
            case AnimationUtil.Direction.LEFT:
                enemyFront.Hide();
                enemySide.Show();
                enemyRear.Hide();
                enemySide.FlipH = true;
                break;
            case AnimationUtil.Direction.TOWARDS:
                enemyFront.Show();
                enemySide.Hide();
                enemyRear.Hide();
                break;
        }
    }

    /// <summary>
    /// Roman Noodles
    /// 5/01/2024
    /// Shoots a bullet in the direction the enemy is facing
    /// </summary>
    private void SpawnBullet(CharacterBody3D target, float bSpeed, int damage, int count, float spread, double lifespan)
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
            // prevents point blank shots from failing
            bullet.GlobalPosition -= pointVector * 1f;
			// set the collision mask of the bullet to the player layer (2) and the hobo layer (5)
			bullet.SetCollisionMaskValue(2, true);
            bullet.SetCollisionMaskValue(5, true);
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
    /// 6/05/2024
    /// Checks surroundings for enemies and targets the closest one
    /// </summary>
    private void CheckSurroundings()
    {
        var allCollisions = nearbyDetection.GetOverlappingBodies();
        Node3D closestCollision = null;
        foreach (var collision in allCollisions)
        {
            if (collision is Hobo || collision is Player)
            {
                if (CheckCanSeeTarget(collision))
                {
                    closestCollision = collision as CharacterBody3D;
                }
            }
        }
        if (closestCollision != null)
        {
            this.SetTarget((CharacterBody3D)closestCollision);
        }
    }

    /// <summary>
    /// Roman Noodles (Adapted from Impulse)
    /// 5/01/2024
    /// Checks if the player is in the line of sight of the enemy
    /// </summary>
    public bool CheckCanSeeTarget(Node3D target)
	{
        Vector3 relativeTargetVector = target.GlobalPosition - this.GlobalPosition;
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
	/// Damages the enemy by the amount specified
	/// </summary>
	public void TakeDamage(int damage)
	{
		damageRandomizer.Play();
		health -= damage;
		if (health <= 0) {
            PlayAnimation("Death"); 
			canMove = false;
			canShoot = false;
		}
		// tell the gamemanager that enemy was hit
		//gameManager.enemyHit(this);
	}

    public void SetTarget(CharacterBody3D target)
    {
        // play the command when the combat encounter starts
        if (!fighting)
        {
            commandRandomizer.Play();
        }
        currentFightTarget = target;
        fighting = true;
    }

    /// <summary>
    /// Roman Noodles
    /// 5/19/2024
    /// Plays the callout sound when a fellow cop was hit
    /// </summary>
    public void Callout()
	{
		calloutRandomizer.Play();
	}
}
