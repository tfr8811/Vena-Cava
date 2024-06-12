using Godot;
using System;
using System.Threading;
using System.Xml.Linq;

/// <summary>
/// Roman Noodles
/// 4/29/2024
/// Controls and manages the player character
/// </summary>
public partial class Player : CharacterBody3D, IDamageable
{
    // movement
    const float GRAVITY = 20f;
    const float JUMP_VELOCITY = 7f;
    const float RUN_SPEED = 8f;
    const float CROUCH_MOVE_SPEED = 4f;
    const float PLAYER_HEIGHT = 2f;
    const float CROUCH_SPEED = 20f;
    const float CROUCH_HEIGHT = 0.75f;
    const float STEP_HEIGHT = 0.5f;
    private float speed;

    // health
    private int maxHealth = 20;
    public int MaxHealth
    {
        get { return this.maxHealth; }
    }
    private int health;
    public int Health
    {
        get { return this.health; }
    }

    // bob variables
    const double BOB_FREQ = 2.0;
    const double BOB_AMP = 0.08;
    private double t_bob = 0.0;

    // fov variables
    const double BASE_FOV = 75.0;
    const double FOV_CHANGE = 1.5;


    // setup the reference objects
    [Export]
    private Node3D head;
    public Node3D Head
    {
        get
        {
            return this.head;
        }
    }

    [Export]
    private Camera3D camera;

    [Signal]
    public delegate void playerHitEventHandler();

    [Signal]
    public delegate void enemyDefeatedEventHandler();

    // Audio
    [Export]
    private AudioStreamPlayer3D damageSound;

    [Export]
    private CollisionShape3D playerCapsule;

    [Export]
    private RayCast3D ceilingChecker;

    // Interaction Raycast
    [Export]
    private RayCast3D interactionRay;

    [Export]
    private RayCast3D stairBelowCheck;

    // handles climbing stairs
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

    private Hud hud;

    private bool headBonked = false;

    private Hobo[] allies;

    private bool dead = false;

    private bool wasOnFloorLastFrame = false;
    private bool snappedToStairsLastFrame = false;
    private Vector3 lastXZVel;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        hud = GetNode<Hud>("Hud");
        allies = new Hobo[0];
        GlobalWorldState.Instance.Player = this;
        health = maxHealth;
        initialSeperationRayDist = Math.Abs(separationRayF.Position.Z);
        lastXZVel = Vector3.Zero;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // fps mouse controls
        // todo add controller support
        float sensitivity = GlobalSettings.Instance.Sensitivity;

        if (@event is InputEventMouseMotion look) {
            // note: X axis mouse movement rotates camera about the Y axis
            head.RotateY(-look.Relative.X * sensitivity);
            camera.RotateX(-look.Relative.Y * sensitivity);
            // clamp the cameras up and down rotation
            camera.Rotation = new Vector3(
                (float) Math.Clamp(camera.Rotation.X, -90 * Math.PI/180, 90 * Math.PI/180), 
                camera.Rotation.Y, 
                camera.Rotation.Z
                );
        }
    }

    public override void _Process(double delta)
    {
        // player dies if they fall out of bounds
        if (this.GlobalPosition.Y < -50)
        {
            TakeDamage(1);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        hud.ShowCrosshair();
        if (interactionRay.IsColliding())
        {
            var collider = interactionRay.GetCollider();
            if (collider is IInteractable)
            {
                float distanceToCollider = (((Node3D)collider).GlobalPosition - this.GlobalPosition).Length();
                if (distanceToCollider < 10f)
                {
                    hud.ShowHand();

                    if (Input.IsActionJustPressed("Interact"))
                    {
                        ((IInteractable)collider).Interact();
                    }
                }
            }

            else if (collider is Hobo)
            {
                if (!((Hobo)collider).recruited)
                {
                    float distanceToCollider = (((Node3D)collider).GlobalPosition - this.GlobalPosition).Length();
                    if (distanceToCollider < 10f)
                    {
                        hud.ShowHand();
                        if (Input.IsActionJustPressed("Interact"))
                        {
                            AddAlly((Hobo)collider);
                        }
                    }
                    else
                    {
                        if (((Hobo)collider).fighting)
                        {
                            if (Input.IsActionJustPressed("Interact"))
                            {
                                ((Hobo)collider).fighting = false;
                            }
                        }
                    }
                }
            }
            else if (collider is Enemy)
            {
                if (Input.IsActionJustPressed("Interact"))
                {
                    for (int i = 0; i < allies.Length; i++)
                    {
                        allies[i].SetTarget((Enemy) collider);
                    }
                }
            }
        }

        // ceiling check
        if (ceilingChecker.IsColliding())
        {
            headBonked = true;
        } else
        {
            headBonked = false;
        }

        // gravity
        if (!IsOnFloor())
        {
            Velocity -= UpDirection * GRAVITY * (float) delta;
        }

        // jump
        else if (Input.IsActionPressed("Jump"))
        {
             Velocity = new Vector3(Velocity.X, JUMP_VELOCITY, Velocity.Z);
        }

        // crouch
        if (Input.IsActionPressed("Crouch"))
        {
            speed = CROUCH_MOVE_SPEED;

            // crouch height adjustment
            ((CapsuleShape3D)playerCapsule.Shape).Height -= CROUCH_SPEED * (float) delta;

            // disable stair stepping
            separationRayF.Disabled = true;
            separationRayL.Disabled = true;
            separationRayR.Disabled = true;
        } else if (!headBonked)
        {
            speed = RUN_SPEED;

            // stand height adjustment
            ((CapsuleShape3D)playerCapsule.Shape).Height += CROUCH_SPEED * (float)delta;
        } else // for partial crouch situations (when head is bonked)
        {
            speed = CROUCH_MOVE_SPEED;
        }
        ((CapsuleShape3D)playerCapsule.Shape).Height = Math.Clamp(((CapsuleShape3D)playerCapsule.Shape).Height, CROUCH_HEIGHT, PLAYER_HEIGHT);
        // enable stair stepping when player is standing
        if (((CapsuleShape3D)playerCapsule.Shape).Height == PLAYER_HEIGHT)
        {
            separationRayF.Disabled = false;
            separationRayL.Disabled = false;
            separationRayR.Disabled = false;
        }

        // movement
        // get the movement direction from the input data
        Vector2 inputDir = Input.GetVector("Left", "Right", "Forward", "Backward");

        Vector3 direction = (head.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        if (IsOnFloor())
        {
            // this value is normalized, so it should be 0 or 1
            if (direction.Length() > 0.1)
            {
                Velocity = new Vector3(
                    (float)Mathf.Lerp(Velocity.X, direction.X * speed, delta * 10.0),
                    Velocity.Y,
                    (float)Mathf.Lerp(Velocity.Z, direction.Z * speed, delta * 10.0)
                    );
            }
            else
            {
                // if the player is not holding a direction, slow their movement
                Velocity = new Vector3(
                    (float)Mathf.Lerp(Velocity.X, direction.X * speed, delta * 7.0),
                    Velocity.Y,
                    (float)Mathf.Lerp(Velocity.Z, direction.Z * speed, delta * 7.0)
                    );
            }
        } else
        {
            if (direction.Length() > 0.1)
            {
                Velocity = new Vector3(
                    (float) Mathf.Lerp(Velocity.X, direction.X * speed, delta * 3.0), 
                    Velocity.Y,
                    (float) Mathf.Lerp(Velocity.Z, direction.Z * speed, delta * 3.0)
                    );
            }
        }


        // head bob
        if (IsOnFloor())
        {
            t_bob += delta * Velocity.Length();
            // make a copy of the camera transform, set its origin, then overwrite the camera's transform
            Transform3D temp_transform = camera.Transform;
            temp_transform.Origin = Headbob(t_bob);
            camera.Transform = temp_transform;
        }

        // FOV
        double velocityClamped = Math.Clamp(Velocity.Length(), 0.5, RUN_SPEED * 2);
        double targetFOV = BASE_FOV + FOV_CHANGE * velocityClamped;
        camera.Fov = (float) Mathf.Lerp(camera.Fov, targetFOV, delta * 8.0);

        // apply the movement
        RotateSeperationRay();
        MoveAndSlide();
        SnapDownStairs();

        if (IsOnFloor())
        {
            wasOnFloorLastFrame = true;
        } else
        {
            wasOnFloorLastFrame = false;
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
        } else
        {
            lastXZVel = xzVel;
        }

        Vector3 xzRayPosF = xzVel.Normalized() * initialSeperationRayDist;
        separationRayF.GlobalPosition = new Vector3(GlobalPosition.X + xzRayPosF.X, separationRayF.GlobalPosition.Y, GlobalPosition.Z + xzRayPosF.Z);

        Vector3 xzRayPosL = xzRayPosF.Rotated(new Vector3(0, 1.0f, 0), Mathf.DegToRad(50));
        separationRayL.GlobalPosition = new Vector3(GlobalPosition.X + xzRayPosL.X, separationRayL.GlobalPosition.Y, GlobalPosition.Z + xzRayPosL.Z);

        Vector3 xzRayPosR = xzRayPosF.Rotated(new Vector3(0,1.0f,0), Mathf.DegToRad(-50));
        separationRayR.GlobalPosition = new Vector3(GlobalPosition.X + xzRayPosR.X, separationRayR.GlobalPosition.Y, GlobalPosition.Z + xzRayPosR.Z);

        slopeCheckF.ForceRaycastUpdate();
        slopeCheckL.ForceRaycastUpdate();
        slopeCheckR.ForceRaycastUpdate();

        float maxSlopeDot = new Vector3(0, 1, 0).Rotated(new Vector3(1, 0, 0), this.FloorMaxAngle).Dot(new Vector3(0, 1, 0));
        bool anyTooSteep = false;
        if (slopeCheckF.IsColliding() && slopeCheckF.GetCollisionNormal().Dot(new Vector3(0, 1, 0)) < maxSlopeDot) {
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
    /// Roman Noodles (Adapted from LegionGames)
    /// 4/29/2024
    /// Returns the relative headbob position
    /// </summary>
    private Vector3 Headbob(double time)
    {
        Vector3 pos = new Vector3(
            (float)(Math.Sin(time * BOB_FREQ/2) * BOB_AMP), 
            (float) (Math.Sin(time * BOB_FREQ) * BOB_AMP), 
            0
            );
        return pos;
    }

    /// <summary>
    /// Roman Noodles
    /// 2/6/2024
    /// Damages the player by the amount specified
    /// </summary>
    public void TakeDamage(int damage)
    {
        health -= damage;
        hud.Update(this);
        if (health <= 0) {
            dead = true;
            GlobalSceneManager.Instance.GameOver();
        } else
        {
            damageSound.Play();
        }
        // emit the player hit signal
        EmitSignal("playerHit");
        hud.DamageFlash();
    }

    public bool IsDead()
    {
        return dead;
    }

    /// <summary>
    /// Roman Noodles
    /// 5/30/2024
    /// Heals the player by the amount specified
    /// </summary>
    public void Heal(int hp)
    {
        health += hp;
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        hud.Update(this);
    }

    /// <summary>
    /// Roman Noodles
    /// 5/19/24
    /// Gets the players head height for the enemies to aim at
    /// </summary>
    public float GetHeadHeight()
    {
        return GlobalPosition.Y + ((CapsuleShape3D)playerCapsule.Shape).Height/2 - 0.3f;
    }

    /// <summary>
    /// Roman Noodles
    /// 6/3/2024
    /// Adds an ally to the player's army
    /// </summary>
    public void AddAlly(Hobo ally)
    {
        ally.recruited = true;
        // add the new hobo to the arrays
        Hobo[] newAllies = new Hobo[allies.Length + 1];
        for (int i = 0; i < allies.Length; i++)
        {
            newAllies[i] = allies[i];
        }
        newAllies[allies.Length] = ally;
        allies = newAllies;
        // swap to the new ally
        // SwapToIndex(allies.Length - 1);
    }
}
