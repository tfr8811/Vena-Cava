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
    private bool dead = false;

    private float facingAngle = 0.0f;

    Player player;

    RandomNumberGenerator rng;

    // handles climbing stairs
    const float STEP_HEIGHT = 0.5f;
    [Export]
    private RayCast3D stairBelowCheck;
    [Export]
    CollisionShape3D separationRayF;
    [Export]
    RayCast3D slopeCheckF;
    [Export]
    CollisionShape3D separationRayL;
    [Export]
    RayCast3D slopeCheckL;
    [Export]
    CollisionShape3D separationRayR;
    [Export]
    RayCast3D slopeCheckR;
    float initialSeperationRayDist;
    private bool wasOnFloorLastFrame = false;
    private bool snappedToStairsLastFrame = false;
    private Vector3 lastXZVel;

    public override void _Ready()
	{
		navAgent = (NavigationAgent3D)GetNode(navPath);
		sightRaycast = (RayCast3D)GetNode(sightPath);
        fireDelay = maxFireDelay;
        postReloadDelay = maxPostReloadDelay;
        PlayAnimation("Idle");
		ammo = maxAmmo;
        player = GlobalWorldState.Instance.Player;

        rng = new RandomNumberGenerator();

        initialSeperationRayDist = Math.Abs(separationRayF.Position.Z);
        lastXZVel = Vector3.Zero;
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

        HandleAnimations();
    }

    public void FightTarget(CharacterBody3D target)
	{
		// navigate to player
		if (IsInstanceValid(target))
		{
            if (target is IDamageable && ((IDamageable) target).IsDead())
            {
                fighting = false;
            }
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
			} else
            {
                // face target
                facingAngle = (new Vector2(targetRelativePosition.X, targetRelativePosition.Z)).Angle();
            }


			// SHOOT
			// fires gun when player is in sight and close enough to shoot
			if (canShoot && shootRadius > targetRelativePosition.Length() && CheckCanSeeTarget(target))
			{
                
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
            // apply the movement
            RotateSeperationRay();
            MoveAndSlide();
            SnapDownStairs();

            if (IsOnFloor())
            {
                wasOnFloorLastFrame = true;
            }
            else
            {
                wasOnFloorLastFrame = false;
            }
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
    }

    /// <summary>
    /// Roman Noodles (Adapted from Majikayo Games)
    /// 6/10/2024
    /// </summary>
    private void SnapDownStairs()
    {
        bool didSnap = false;
        if (!IsOnFloor() && Velocity.Y <= 0 && (wasOnFloorLastFrame || snappedToStairsLastFrame) && stairBelowCheck.IsColliding())
        {
            PhysicsTestMotionResult3D motionResult = new PhysicsTestMotionResult3D();
            PhysicsTestMotionParameters3D motionParameters = new PhysicsTestMotionParameters3D();
            motionParameters.From = this.GlobalTransform;
            motionParameters.Motion = new Vector3(0, -STEP_HEIGHT, 0);
            if (PhysicsServer3D.BodyTestMotion(this.GetRid(), motionParameters, motionResult))
            {
                float translateY = motionResult.GetTravel().Y;
                this.Position = new Vector3(this.Position.X, this.Position.Y + translateY, this.Position.Z);
                ApplyFloorSnap();
                didSnap = true;
            }
        }
        snappedToStairsLastFrame = didSnap;
    }

    /// <summary>
    /// Roman Noodles (Adapted from Majikayo Games)
    /// 6/10/2024
    /// </summary>
    private void RotateSeperationRay()
    {
        Vector3 xzVel = Velocity * new Vector3(1, 0, 1);

        if (xzVel.Length() < 0.1f)
        {
            xzVel = lastXZVel;
        }
        else
        {
            lastXZVel = xzVel;
        }

        Vector3 xzRayPosF = xzVel.Normalized() * initialSeperationRayDist;
        separationRayF.GlobalPosition = new Vector3(GlobalPosition.X + xzRayPosF.X, separationRayF.GlobalPosition.Y, GlobalPosition.Z + xzRayPosF.Z);

        Vector3 xzRayPosL = xzRayPosF.Rotated(new Vector3(0, 1.0f, 0), Mathf.DegToRad(50));
        separationRayL.GlobalPosition = new Vector3(GlobalPosition.X + xzRayPosL.X, separationRayL.GlobalPosition.Y, GlobalPosition.Z + xzRayPosL.Z);

        Vector3 xzRayPosR = xzRayPosF.Rotated(new Vector3(0, 1.0f, 0), Mathf.DegToRad(-50));
        separationRayR.GlobalPosition = new Vector3(GlobalPosition.X + xzRayPosR.X, separationRayR.GlobalPosition.Y, GlobalPosition.Z + xzRayPosR.Z);

        slopeCheckF.ForceRaycastUpdate();
        slopeCheckL.ForceRaycastUpdate();
        slopeCheckR.ForceRaycastUpdate();

        float maxSlopeDot = new Vector3(0, 1, 0).Rotated(new Vector3(1, 0, 0), this.FloorMaxAngle).Dot(new Vector3(0, 1, 0));
        bool anyTooSteep = false;
        if (slopeCheckF.IsColliding() && slopeCheckF.GetCollisionNormal().Dot(new Vector3(0, 1, 0)) < maxSlopeDot)
        {
            anyTooSteep = true;
        }
        if (slopeCheckL.IsColliding() && slopeCheckL.GetCollisionNormal().Dot(new Vector3(0, 1, 0)) < maxSlopeDot)
        {
            anyTooSteep = true;
        }
        if (slopeCheckR.IsColliding() && slopeCheckR.GetCollisionNormal().Dot(new Vector3(0, 1, 0)) < maxSlopeDot)
        {
            anyTooSteep = true;
        }

        if (anyTooSteep)
        {
            separationRayF.Disabled = true;
            separationRayL.Disabled = true;
            separationRayR.Disabled = true;
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
            if ((collision is Hobo && !((Hobo)collision).IsDead()) || collision is Player)
            {
                if (CheckCanSeeTarget(collision))
                {
                    if (closestCollision != null)
                    {
                        Vector3 collisionDist = this.GlobalPosition - collision.GlobalPosition;
                        Vector3 closestCollisionDist = this.GlobalPosition - closestCollision.GlobalPosition;
                        if (collisionDist.Length() < closestCollisionDist.Length())
                        {
                            closestCollision = collision as CharacterBody3D;
                        }
                    } else
                    {
                        closestCollision = collision as CharacterBody3D;
                    }
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
            dead = true;
        }
		// tell the gamemanager that enemy was hit
		//gameManager.enemyHit(this);
	}

    public bool IsDead()
    {
        return dead;
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

    public void HandleAnimations()
    {
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
}
