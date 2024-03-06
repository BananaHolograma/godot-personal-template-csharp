namespace GameRoot;

using System.Collections.Generic;
using Godot;
[GlobalClass]
public partial class Jump : Motion
{
    [Export] public float VelocityBarrierToFall = -10f;
    [Export] public float AirControlSpeed = 7f;
    [Export] public float Acceleration = 12.5f;
    [Export] public int JumpTimes = 1;
    [Export] public float HeightReducedByJump = .2f;
    [Export] public float OverrideJumpGravity = 0f;
    [Export] public float OverrideFallGravity = 0f;
    [Export]
    public float JumpHeight
    {
        get => jumpHeight;
        set
        {
            jumpHeight = Mathf.Max(0, value);

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
    [Export] public float WallDetectionAfterFrames = 25f;
    public Timer WallDetectionTimer;
    public bool WallDetectionActive = false;

    private float jumpHeight = 2f;
    private float jumpTimeToPeak = .45f;
    private float jumpTimeToFall = .4f;

    public float JumpVelocity;
    public float JumpGravity;
    public float FallGravity;

    public int JumpCount = 1;

    public float JumpHorizontalBoost = 0;
    public float JumpVerticalBoost = 0;
    private bool JumpRequested;

    public override void Ready()
    {
        CreateWallDetectionTimer();
    }

    public override void Enter()
    {
        JumpVelocity = CalculateJumpVelocity(JumpHeight, JumpTimeToPeak);
        JumpGravity = CalculateJumpGravity(JumpHeight, JumpTimeToPeak);
        FallGravity = CalculateFallGravity(JumpHeight, JumpTimeToFall);

        Actor.Velocity = Actor.Velocity with { Y = JumpVelocity + JumpHorizontalBoost, Z = Actor.Velocity.Z + JumpHorizontalBoost };
        Actor.MoveAndSlide();

        if (IsInstanceValid(WallDetectionTimer))
            WallDetectionTimer.Start();

        WallNormals = new() { };
    }

    public override void Exit(State _nextState)
    {
        JumpCount = 1;
        JumpHorizontalBoost = 0;
        JumpVerticalBoost = 0;
        WallDetectionActive = false;

        if (IsInstanceValid(WallDetectionTimer))
            WallDetectionTimer.Stop();
    }

    public override void PhysicsUpdate(double delta)
    {
        base.PhysicsUpdate(delta);

        JumpRequested = Input.IsActionJustPressed("jump");

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
            if (Actor.Velocity.Y > 0)
            {
                ApplyGravity(JumpGravity, delta);
            }
            else
            {
                ApplyGravity(FallGravity, delta);

                if (VelocityBarrierToFall < 0 && Actor.Velocity.Y < VelocityBarrierToFall)
                {
                    FSM.ChangeStateTo("Fall");
                }
            }

            Move(AirControlSpeed, delta, Acceleration);

            if (JumpRequested && JumpTimes > 1 && JumpCount < JumpTimes)
            {
                Actor.Velocity = Actor.Velocity with { Y = CalculateJumpVelocity(JumpHeight - (JumpTimes * HeightReducedByJump), JumpTimeToPeak) };
                JumpCount += 1;
            }

            DetectWallRun();
        }

        Actor.MoveAndSlide();
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
        return 2f * Mathf.Max(0, jumpHeight) / (timeToPeak * Actor.UpDirection.Y);
    }

    private float CalculateJumpGravity(float jumpHeight, float timeToPeak)
    {
        return OverrideJumpGravity > 0 ? OverrideJumpGravity : 2f * Mathf.Max(0, jumpHeight) / Mathf.Pow(timeToPeak, 2);
    }

    private float CalculateFallGravity(float jumpHeight, float timeToFall)
    {
        return OverrideFallGravity > 0 ? OverrideFallGravity : 2f * Mathf.Max(0, jumpHeight) / Mathf.Pow(timeToFall, 2);
    }
}