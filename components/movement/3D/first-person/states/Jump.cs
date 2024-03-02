namespace GameRoot;

using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class Jump : Motion
{
    [Export]
    public float VelocityBarrierToFall = -15f;
    [Export]
    public float AirControlSpeed = 7.5f;
    [Export]
    public int JumpTimes = 1;
    [Export]
    public float OverrideJumpGravity = 0f;
    [Export]
    public float OverrideFallGravity = 0f;
    [Export]
    public float JumpHeight
    {
        get => jumpHeight;
        set
        {
            jumpHeight = value;

            if (IsNodeReady())
                JumpVelocity = CalculateJumpVelocity(jumpHeight, jumpTimeToPeak);
        }
    }

    [Export]
    public float JumpTimeToPeak
    {
        get => jumpTimeToPeak;
        set
        {
            jumpTimeToPeak = value;

            if (IsNodeReady())
                JumpGravity = CalculateJumpGravity(jumpHeight, jumpTimeToPeak);
        }
    }
    [Export]
    public float JumpTimeToFall
    {
        get => jumpTimeToFall;
        set
        {
            jumpTimeToFall = value;

            if (IsNodeReady())
                FallGravity = CalculateFallGravity(jumpHeight, jumpTimeToFall);
        }
    }

    [ExportGroup("Wall run")]
    [Export]
    public float WallDetectionAfterFrames = 25f;

    private float jumpHeight = 2f;
    private float jumpTimeToPeak = .45f;
    private float jumpTimeToFall = .4f;

    public float JumpVelocity;
    public float JumpGravity;
    public float FallGravity;

    public int JumpCount = 1;

    public bool WallDetectionActive = false;
    public Timer WallDetectionTimer;

    public RayCast3D RightWallDetector;
    public RayCast3D LeftWallDetector;
    public RayCast3D FrontWallDetector;

    public Dictionary<string, Vector3> WallNormals = new() { };

    public override void Ready()
    {
        CreateWallDetectionTimer();

        RightWallDetector = Actor.GetNode<RayCast3D>("%RightWallDetector");
        LeftWallDetector = Actor.GetNode<RayCast3D>("%LeftWallDetector");
        FrontWallDetector = Actor.GetNode<RayCast3D>("%FrontWallDetector");
    }

    public override void Enter()
    {
        JumpVelocity = CalculateJumpVelocity(JumpHeight, JumpTimeToPeak);
        JumpGravity = CalculateJumpGravity(JumpHeight, JumpTimeToPeak);
        FallGravity = CalculateFallGravity(JumpHeight, JumpTimeToFall);

        Actor.Velocity = Actor.Velocity with { Y = JumpVelocity };
        Actor.MoveAndSlide();

        if (IsInstanceValid(WallDetectionTimer))
            WallDetectionTimer.Start();

        WallNormals = new() { };
    }

    public override void Exit(State _nextState)
    {
        JumpCount = 1;
        WallDetectionActive = false;

        if (IsInstanceValid(WallDetectionTimer))
            WallDetectionTimer.Stop();
    }

    public override void PhysicsUpdate(double delta)
    {
        base.PhysicsUpdate(delta);

        if (Actor.IsOnFloor())
        {
            if (TransformedInput.InputDirection.IsZeroApprox())
            {
                FSM.ChangeStateTo("Idle");
            }
            else
            {
                FSM.ChangeStateTo("Walk");
            }
        }
        else
        {
            Vector3 oppositeUpDirection = GetCharacterUpDirectionOppositeVector(Actor);

            if (Actor.Velocity.Y > 0)
            {
                Actor.Velocity += oppositeUpDirection * JumpGravity * (float)delta;
            }
            else
            {
                Actor.Velocity += oppositeUpDirection * FallGravity * (float)delta;

                if (VelocityBarrierToFall < 0 && Actor.Velocity.Y < VelocityBarrierToFall)
                {
                    FSM.ChangeStateTo("Fall");
                }
            }

            Actor.Velocity = Actor.Velocity with
            {
                X = (float)Mathf.Lerp(Actor.Velocity.X, TransformedInput.WorldCoordinateSpaceDirection.X * AirControlSpeed, delta),
                Z = (float)Mathf.Lerp(Actor.Velocity.Z, TransformedInput.WorldCoordinateSpaceDirection.Z * AirControlSpeed, delta),
            };

            if (Input.IsActionJustPressed("jump") && JumpTimes > 1 && JumpCount < JumpTimes)
            {
                Actor.Velocity = Actor.Velocity with { Y = JumpVelocity };
                JumpCount += 1;
            }

            if (Actor.WallRun && WallDetectionActive && WallDetected())
            {
                FSM.ChangeStateTo("WallRun");
            }
        }

        Actor.MoveAndSlide();
    }

    private bool WallDetected()
    {
        UpdateWallNormals();
        return !Actor.IsOnFloor() && (RightWallDetector.IsColliding() || LeftWallDetector.IsColliding() || FrontWallDetector.IsColliding());
    }

    private void UpdateWallNormals()
    {
        RightWallDetector.ForceRaycastUpdate();
        LeftWallDetector.ForceRaycastUpdate();
        FrontWallDetector.ForceRaycastUpdate();

        WallNormals[RightWallDetector.Name] = RightWallDetector.IsColliding() ? RightWallDetector.GetCollisionNormal() : Vector3.Zero;
        WallNormals[LeftWallDetector.Name] = LeftWallDetector.IsColliding() ? LeftWallDetector.GetCollisionNormal() : Vector3.Zero;
        WallNormals[FrontWallDetector.Name] = FrontWallDetector.IsColliding() ? FrontWallDetector.GetCollisionNormal() : Vector3.Zero;
    }

    private void CreateWallDetectionTimer()
    {
        if (WallDetectionTimer == null)
        {
            WallDetectionTimer = new Timer
            {
                Name = "WallDetectionTimer",
                WaitTime = WallDetectionAfterFrames / Engine.PhysicsTicksPerSecond,
                ProcessCallback = Timer.TimerProcessCallback.Physics,
                Autostart = false,
                OneShot = true
            };

            AddChild(WallDetectionTimer);
            WallDetectionTimer.Timeout += () => WallDetectionActive = true;
        }

    }

    private float CalculateJumpVelocity(float jumpHeight, float timeToPeak)
    {
        return 2f * jumpHeight / (timeToPeak * Actor.UpDirection.Y);
    }

    private float CalculateJumpGravity(float jumpHeight, float timeToPeak)
    {
        return OverrideJumpGravity > 0 ? OverrideJumpGravity : 2f * jumpHeight / Mathf.Pow(timeToPeak, 2);
    }

    private float CalculateFallGravity(float jumpHeight, float timeToFall)
    {
        return OverrideFallGravity > 0 ? OverrideFallGravity : 2f * jumpHeight / Mathf.Pow(timeToFall, 2);
    }
}