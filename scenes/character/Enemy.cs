using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 5/01/2024
/// Controls and Manages the cop enemy
/// </summary>

public partial class Enemy : CharacterBody3D, IDamageable
{
	[Export]
	private AnimatedSprite3D enemySprite;

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

	RandomNumberGenerator rng;

	public override void _Ready()
	{
		navAgent = (NavigationAgent3D)GetNode(navPath);
		sightRaycast = (RayCast3D)GetNode(sightPath);
        fireDelay = maxFireDelay;
        postReloadDelay = maxPostReloadDelay;
		enemySprite.Play("Idle");
		ammo = maxAmmo;

        rng = new RandomNumberGenerator();
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
    }

	public override void _PhysicsProcess(double delta)
	{

		// reset velocity
		Velocity = Vector3.Zero;

		Player player = GlobalWorldState.Instance.Player;

		// navigate to player
		if (IsInstanceValid(player))
		{
			// FACE PLAYER - enemy always faces player as the sprites are 2d
			LookAt(new Vector3(player.GlobalPosition.X, GlobalPosition.Y, player.GlobalPosition.Z), Vector3.Up);
			
			// MOVE
			Vector3 playerRelativePosition = player.GlobalPosition - this.GlobalPosition;
			// chases player when they are in site or if they have already spotted the player
			if (canMove && (hasSeenPlayer || CheckCanSeeTarget(playerRelativePosition, player)))
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
					Velocity = (nextNavPoint - GlobalTransform.Origin).Normalized() * speed;
				} else
				{
					// The enemy is not moving:
					enemySprite.Play("Idle");
				}
			}

			// SHOOT
			// fires gun when player is in sight and close enough to shoot
			if (canShoot && shootRadius > playerRelativePosition.Length() && CheckCanSeeTarget(playerRelativePosition, player))
			{
				if (ammo > 0)
				{
					if (postReloadDelay <= 0 && fireDelay <= 0)
					{
						// SHOOT THE BULLET
						enemySprite.Play("Shoot");
						shootSound.Play();
						ammo -= 1;
                        SpawnBullet(bulletSpeed, bulletDamage, projectileCount, projectileSpread, maxBulletLifespan);
                        canShoot = false;
						canMove = false;
						fireDelay = maxFireDelay;
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
				if (!enemySprite.IsPlaying() && fireDelay <= 0)
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
					ammo = maxAmmo;
					enemySprite.Play("Idle");
					canShoot = true;
					canMove = true;

                    // postReloadDelay exists so the enemy at some point tries to get closer to the player
                    postReloadDelay = maxPostReloadDelay;
				}
			}

			// death animation handler
			if (enemySprite.Animation == "Death")
			{
				if (!enemySprite.IsPlaying())
				{
					// tell the gamemanager that this enemy was defeated
					player.EmitSignal("enemyDefeated");
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
    private void SpawnBullet(float bSpeed, int damage, int count, float spread, double lifespan)
    {
		for (int i = 0; i < count; i++)
		{
			Bullet bullet = (Bullet)psBullet.Instantiate();
			GetNode("/root/World").AddChild(bullet);
			// calculate the bullets trajectory
			Player player = GlobalWorldState.Instance.Player;
			Vector3 pointVector = new Vector3(
				player.GlobalPosition.X - GlobalPosition.X, 
				player.GetHeadHeight() - (GlobalPosition.Y + shotHeight), 
				player.GlobalPosition.Z - GlobalPosition.Z
				);
			pointVector = pointVector.Normalized();
			bullet.GlobalPosition = GlobalPosition + new Vector3(0, shotHeight, 0);
			// prevents point blank shots from failing
			bullet.GlobalPosition -= pointVector * 1f;
			// set the collision mask of the bullet to the player layer (2)
			bullet.SetCollisionMaskValue(2, true);
            // guns that shoot in bursts are stronger close up
            bullet.BulletTimer = lifespan * (((float)i + 1f) / (float)count);
            bullet.Damage = damage;
			// apply spread
			if (spread > 0)
			{
				Vector3 vSpread = new Vector3(rng.Randf() - 0.5f, rng.Randf() - 0.5f, rng.Randf() - 0.5f);
				vSpread = vSpread.Normalized() * spread;
				bullet.Velocity = (pointVector * bSpeed) + vSpread;
			} else
			{
				bullet.Velocity = (pointVector * bSpeed);
            }
		}
	}

	/// <summary>
	/// Roman Noodles (Adapted from Impulse)
	/// 5/01/2024
	/// Checks if the player is in the line of sight of the enemy
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
	/// Damages the enemy by the amount specified
	/// </summary>
	public void TakeDamage(int damage)
	{
		damageRandomizer.Play();
		health -= damage;
		if (health <= 0) { 
			enemySprite.Play("Death"); 
			canMove = false;
			canShoot = false;
		}
		// tell the gamemanager that enemy was hit
		//gameManager.enemyHit(this);
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
