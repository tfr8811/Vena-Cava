using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 4/29/2024
/// Controls and manages the player character
/// </summary>
public partial class Player : CharacterBody3D, IDamageable
{
    // movement
    const float GRAVITY = 10f;
    const float JUMP_VELOCITY = 4.5f;
    const float WALK_SPEED = 10f;
    const float SPRINT_SPEED = 15f;
    private float speed;

    // health
    private int maxHealth = 10;
    public int MaxHealth
    {
        get { return this.health; }
    }
    private int health = 10;
    public int Health
    {
        get { return this.health; }
    }


    const float SENSITIVITY = 0.001f;

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

    [Export]
    private Camera3D camera;

    // send out a signal when the player is hit
    [Signal]
    public delegate void playerHitEventHandler();

    // Audio
    [Export]
    private AudioStreamPlayer3D damageSound;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        // this line of code is required to prevent the player from flying into the stratosphere
        // Velocity = new Vector3(1, Velocity.Y, 1);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // fps mouse controls
        // todo add controller support
        if (@event is InputEventMouseMotion look) {
            // note: X axis mouse movement rotates camera about the Y axis
            head.RotateY(-look.Relative.X * SENSITIVITY);
            camera.RotateX(-look.Relative.Y * SENSITIVITY);
            // clamp the cameras up and down rotation
            camera.Rotation = new Vector3(
                (float) Math.Clamp(camera.Rotation.X, -40 * Math.PI/180, 60 * Math.PI/180), 
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
        // update the forward vector of the player

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

        // sprint
        if (Input.IsActionPressed("Sprint"))
        {
            speed = SPRINT_SPEED;
        } else
        {
            speed = WALK_SPEED;
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
                Velocity = new Vector3(direction.X * speed, Velocity.Y, direction.Z * speed);
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
                    (float) Mathf.Lerp(Velocity.X, direction.X * speed, delta * 2.0), 
                    Velocity.Y,
                    (float) Mathf.Lerp(Velocity.Z, direction.Z * speed, delta * 2.0)
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
        double velocityClamped = Math.Clamp(Velocity.Length(), 0.5, SPRINT_SPEED * 2);
        double targetFOV = BASE_FOV + FOV_CHANGE * velocityClamped;
        camera.Fov = (float) Mathf.Lerp(camera.Fov, targetFOV, delta * 8.0);

        // apply the movement
        MoveAndSlide();
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
        if (health <= 0) { 
            GetTree().ChangeSceneToFile("res://assets/Scenes/GameOver.tscn"); 
        } else
        {
            damageSound.Play();
        }
        // emit the player hit signal
        EmitSignal("playerHit");
    }
}
