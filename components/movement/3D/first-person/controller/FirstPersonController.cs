namespace GameRoot;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using GodotExtensions;

public partial class FirstPersonController : CharacterBody3D
{
    #region Exports
    [ExportGroup("Mechanics")]
    [Export] public bool Jump = true;
    [Export] public bool WallJump = false;
    [Export] public bool WallRun = false;
    [Export] public bool Run = true;
    [Export] public bool Crouch = true;
    [Export] public bool Crawl = false;
    [Export] public bool Slide = false;

    [ExportGroup("Camera Parameters")]
    [Export(PropertyHint.Range, "0,1,0.01")] public float CameraSensitivity = .45f;
    [Export] public float MouseSensitivity = 3f;
    [Export] public double CameraRotationLimit = Math.PI / 2;
    [Export] public int CameraJitterSmoothing = 18;
    [Export] public Array<float> RunCameraFovRange = new() { 2f, 75f, 85f, 8 };
    [Export] public Array<float> WallRunCameraFovRange = new() { 2f, 75f, 95f, 8 };

    [ExportGroup("Head bobbing")]
    [Export] public bool HeadBobbingEnabled = true;
    [Export] public float HeadBobAmplitude = .08f;
    [Export] public float HeadBobFrequency = 2f;

    [ExportGroup("Swing head")]
    [Export] public bool SwingHeadEnabled = true;
    [Export] public float SwingHeadRotation = 3f;
    [Export] public float SwingHeadRotationLerp = .05f;
    [Export] public float SwingHeadRecoveryLerp = .15f;
    #endregion

    public GameEvents GameEvents;
    public FiniteStateMachine FSM;
    public Camera3D Camera;
    public Node3D Head;
    public Node3D Eyes;

    #region Collisions
    public CollisionShape3D StandCollisionShape;
    public CollisionShape3D CrouchCollisionShape;
    public CollisionShape3D CrawlCollisionShape;
    public ShapeCast3D CeilShapeDetector;

    public RayCast3D RightWallDetector;
    public RayCast3D LeftWallDetector;
    public RayCast3D FrontWallDetector;

    public RayCast3D FloorDetectorRayCast;
    #endregion
    public Vector3 OriginalEyesPosition;

    public bool Locked = false;
    public float HeadBobTimePassed = 0f;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Locked && Input.MouseMode.Equals(Input.MouseModeEnum.Captured) && @event is InputEventMouseMotion motion)
            RotateCamera(motion.Relative.X, motion.Relative.Y);

        if (Input.IsActionJustPressed("ui_cancel"))
            SwitchMouseMode();
    }

    public override void _Ready()
    {
        GameEvents = this.GetAutoloadNode<GameEvents>("GameEvents");

        FSM = GetNode<FiniteStateMachine>("FiniteStateMachine");
        FSM.RegisterTransition("WalkToRun", new WalkToRunTransition());
        FSM.RegisterTransition("RunToWalk", new RunToWalkTransition());
        FSM.RegisterTransition("JumpToWallRun", new AnyToWallRunTransition());
        FSM.RegisterTransition("FallToWallRun", new AnyToWallRunTransition());
        FSM.RegisterTransition("WallRunToJump", new WallRunToJumpTransition());
        FSM.RegisterTransition("WalkToVault", new AnyToVaultTransition());
        FSM.RegisterTransition("RunToVault", new AnyToVaultTransition());
        FSM.StateChanged += OnStateChanged;

        Camera = GetNode<Camera3D>("%Camera3D");
        Head = GetNode<Node3D>("Head");
        Eyes = GetNode<Node3D>("Head/Eyes");

        StandCollisionShape = GetNode<CollisionShape3D>("StandCollisionShape");
        CrouchCollisionShape = GetNode<CollisionShape3D>("CrouchCollisionShape");
        CrawlCollisionShape = GetNode<CollisionShape3D>("CrawlCollisionShape");
        CeilShapeDetector = GetNode<ShapeCast3D>("CeilShapeDetector");

        RightWallDetector = GetNode<RayCast3D>("%RightWallDetector");
        LeftWallDetector = GetNode<RayCast3D>("%LeftWallDetector");
        FrontWallDetector = GetNode<RayCast3D>("%FrontWallDetector");

        FloorDetectorRayCast = GetNode<RayCast3D>("FootstepManager/FloorDetectorRayCast");

        OriginalEyesPosition = Eyes.Transform.Origin;

        Input.MouseMode = Input.MouseModeEnum.Captured;

        GameEvents.LockPlayer += OnLockPlayer;
        GameEvents.UnlockPlayer += OnUnlockPlayer;
    }

    private void OnStateChanged(State _from, State _to)
    {
        UpdateCollisions();
    }

    public override void _PhysicsProcess(double delta)
    {

        HeadBobbing(delta);
        CameraFov(delta);
        SwingHead();

        if (Velocity.Y > 0)
            SmoothCameraJitter(delta);
    }

    public void HeadBobbing(double delta)
    {
        if (HeadBobbingEnabled && IsOnFloor())
        {
            if (Velocity.IsNotZeroApprox() && FSM.CurrentStateIsNot(new string[] { "Jump", "Slide", "Fall" }))
            {
                HeadBobTimePassed += (float)delta * Velocity.Length() * (IsOnFloor() ? 1f : 0);

                Transform3D newTransform = Eyes.Transform;
                newTransform.Origin = newTransform.Origin with
                {
                    X = Mathf.Cos(HeadBobTimePassed * HeadBobFrequency / 2f) * HeadBobAmplitude,
                    Y = Mathf.Sin(HeadBobTimePassed * HeadBobFrequency) * HeadBobAmplitude,
                };

                Eyes.Transform = newTransform;
            }
        }
    }

    public void RotateCamera(float relativeX, float relativeY)
    {
        float mouseSensitivity = MouseSensitivity / 1000f;

        float twistInput = relativeX * mouseSensitivity;
        float pitchInput = relativeY * mouseSensitivity;

        float targetRotationY = Rotation.Y - twistInput; // Body rotation
        float targetRotationX = Head.Rotation.X - pitchInput; // Head & Neck rotation
        targetRotationX = (float)Mathf.Clamp(targetRotationX, -CameraRotationLimit, CameraRotationLimit);

        Rotation = Rotation with { Y = Mathf.LerpAngle(Rotation.Y, targetRotationY, CameraSensitivity) };
        Head.Rotation = Head.Rotation with { X = Mathf.LerpAngle(Head.Rotation.X, targetRotationX, CameraSensitivity) };
    }

    public void SmoothCameraJitter(double delta)
    {
        Eyes.GlobalPosition = Eyes.GlobalPosition with
        {
            X = Head.GlobalPosition.X,
            Y = (float)Mathf.Lerp(Eyes.GlobalPosition.Y, Head.GlobalPosition.Y, CameraJitterSmoothing * delta),
            Z = Head.GlobalPosition.Z
        };

        Eyes.GlobalPosition = Eyes.GlobalPosition with { Y = Mathf.Clamp(Eyes.GlobalPosition.Y, -Head.GlobalPosition.Y - 1, Head.GlobalPosition.Y + 1) };
    }

    public void CameraFov(double delta)
    {
        Camera.Fov = (string)FSM.CurrentState.Name switch
        {
            "Run" => Mathf.Lerp(Camera.Fov, RunCameraFovRange[2], (float)delta * RunCameraFovRange[3]),
            "WallRun" => Mathf.Lerp(Camera.Fov, WallRunCameraFovRange[2], (float)delta * WallRunCameraFovRange[3]),
            _ => Mathf.Lerp(Camera.Fov, RunCameraFovRange[1], (float)delta * RunCameraFovRange[3]),
        };
    }

    public void SwingHead()
    {
        if (SwingHeadEnabled)
        {
            if (FSM.CurrentState is Motion motion)
            {
                Vector2 direction = motion.TransformedInput.InputDirection;
                bool isLeftOrRight = new[] { Vector2.Left, Vector2.Right }.Any(dir => dir == direction);

                if (isLeftOrRight)
                {
                    Head.Rotation = Head.Rotation with { Z = Mathf.LerpAngle(Head.Rotation.Z, -Mathf.Sign(direction.X) * Mathf.DegToRad(SwingHeadRotation), SwingHeadRotationLerp) };
                }
                else
                {
                    Head.Rotation = Head.Rotation with { Z = Mathf.LerpAngle(Head.Rotation.Z, 0, SwingHeadRotationLerp) };
                }

            }
        }
    }
    public void UpdateCollisions()
    {
        switch (FSM.CurrentState.Name)
        {
            case "Idle":
            case "Walk":
            case "Run":
                StandCollisionShape.Disabled = false;
                CrouchCollisionShape.Disabled = true;
                CrawlCollisionShape.Disabled = true;

                RightWallDetector.Enabled = false;
                LeftWallDetector.Enabled = false;
                FrontWallDetector.Enabled = true;

                FloorDetectorRayCast.Enabled = true;
                break;

            case "Jump":
            case "WallRun":
            case "Fall":
                FloorDetectorRayCast.Enabled = false;
                break;

            case "Crouch":
            case "Slide":
                StandCollisionShape.Disabled = true;
                CrouchCollisionShape.Disabled = false;
                CrawlCollisionShape.Disabled = true;

                RightWallDetector.Enabled = false;
                LeftWallDetector.Enabled = false;
                FrontWallDetector.Enabled = false;
                break;

            case "Crawl":
                StandCollisionShape.Disabled = true;
                CrouchCollisionShape.Disabled = true;
                CrawlCollisionShape.Disabled = false;

                RightWallDetector.Enabled = false;
                LeftWallDetector.Enabled = false;
                FrontWallDetector.Enabled = false;
                break;

            default:
                StandCollisionShape.Disabled = false;
                CrouchCollisionShape.Disabled = true;
                CrawlCollisionShape.Disabled = true;

                RightWallDetector.Enabled = WallRun;
                LeftWallDetector.Enabled = WallRun;
                FrontWallDetector.Enabled = WallRun;

                FloorDetectorRayCast.Enabled = true;
                break;
        }

    }

    public void SwitchMouseMode()
    {
        if (Input.MouseMode.Equals(Input.MouseModeEnum.Captured))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }

    public void OnUnlockPlayer()
    {
        Locked = false;
        FSM.UnlockStateMachine();
    }

    public void OnLockPlayer()
    {
        Locked = true;
        FSM.LockStateMachine();
    }
}