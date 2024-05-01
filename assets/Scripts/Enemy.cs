using Godot;
using System;

public partial class Enemy : CharacterBody3D
{
    // player reference
    private CharacterBody3D player = null;

    [Export]
    private NodePath playerPath;

    // navigation agent reference
    private NavigationAgent3D navAgent;

    [Export]
    private NodePath navPath;

    // movement
    const float GRAVITY = 10f;
    const float SPEED = 6f;

    // shooting
    const float BULLET_SPEED = 40.0f;

    // AI stuff
    private float detectionRadius = 5.0f;
    RayCast2D enemyRaycast;
    private double fireDelay = 5.0;
    private double delay;
    private double lockOnDelay = 1.0;

    // instance the bullet - Tom
    PackedScene psBullet = GD.Load<PackedScene>("res://assets/Scenes/Bullet.tscn");

    public override void _Ready()
    {
        player = (CharacterBody3D) GetNode(playerPath);
        navAgent = (NavigationAgent3D) GetNode(navPath);
    }

    public override void _PhysicsProcess(double delta)
    {
        // gravity
        if (!IsOnFloor())
        {
            Velocity -= UpDirection * GRAVITY * (float)delta;
        }

        navAgent.TargetPosition = player.GlobalTransform.Origin;
        Vector3 nextNavPoint = navAgent.GetNextPathPosition();
        Velocity = (nextNavPoint - GlobalTransform.Origin).Normalized() * SPEED;

        LookAt(new Vector3(player.GlobalPosition.X, GlobalPosition.Y, player.GlobalPosition.Z), Vector3.Up);

        MoveAndSlide();
    }

    public void SpawnBullet(float speed)
    {
        Bullet bullet = (Bullet)psBullet.Instantiate();
        GetNode("/root").AddChild(bullet);
        // set the position of the bullet in front of the player
        Vector3 pointVector = -GlobalTransform.Basis.Z;
        bullet.GlobalPosition = GlobalPosition;
        bullet.GlobalPosition += pointVector * 2;


        bullet.Velocity = pointVector * speed;
    }
}
