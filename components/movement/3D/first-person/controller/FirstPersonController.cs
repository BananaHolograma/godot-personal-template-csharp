namespace GameRoot;

using System;
using Godot;
using Godot.Collections;
using GodotExtensions;

public partial class FirstPersonController : CharacterBody3D
{
    #region Exports
    [ExportGroup("Mechanics")]
    [Export]
    public bool Jump = true;
    [Export]
    public bool WallJump = false;
    [Export]
    public bool WallRun = false;
    [Export]
    public bool Run = true;
    [Export]
    public bool Crouch = true;
    [Export]
    public bool Crawl = false;
    [Export]
    public bool Slide = false;

    [ExportGroup("Camera Parameters")]
    [Export(PropertyHint.Range, "0,1,0.01")]
    public float CameraSensitivity = .45f;
    [Export]
    public float MouseSensitivity = 3f;
    [Export]
    public double CameraRotationLimit = Math.PI / 2;
    [Export]
    public int CameraJitterSmoothing = 18;
    [Export]
    public Array<float> RunCameraFovRange = new() { 2f, 75f, 85f, 8 };
    [Export]
    public Array<float> WallRunCameraFovRange = new() { 2f, 75f, 95f, 8 };

    [ExportGroup("Head bobbing")]
    [Export]
    public bool HeadBobbingEnabled = true;
    [Export]
    public float HeadBobAmplitude = .08f;
    [Export]
    public float HeadBobFrequency = 2f;

    [ExportGroup("Swing head")]
    [Export]
    public bool SwingHeadEnabled = true;
    [Export]
    public float SwingHeadRotation = 3f;
    [Export]
    public float SwingHeadRotationLerp = .05f;
    [Export]
    public float SwingHeadRecoveryLerp = .15f;
    #endregion

    public GameEvents GameEvents;
    public FiniteStateMachine FSM;
    public Camera3D Camera;
    public Node3D Head;
    public Node3D Eyes;

    public Vector3 OriginalEyesPosition;

    public bool Locked = false;
    public float HeadBobTimePassed = 0;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Locked && Input.MouseMode.Equals(Input.MouseModeEnum.Captured) && @event is InputEventMouseMotion motion)
        {
            RotateCamera(motion.Relative.X, motion.Relative.Y);
        }

        if (Input.IsActionJustPressed("ui_cancel"))
        {
            SwitchMouseMode();
        }
    }
    public override void _Ready()
    {
        GameEvents = this.GetAutoloadNode<GameEvents>("GameEvents");

        FSM = GetNode<FiniteStateMachine>("FiniteStateMachine");
        Camera = GetNode<Camera3D>("%Camera3D");
        Head = GetNode<Node3D>("Head");
        Eyes = GetNode<Node3D>("Head/Eyes");

        OriginalEyesPosition = Eyes.Transform.Origin;

        Input.MouseMode = Input.MouseModeEnum.Captured;

        GameEvents.LockPlayer += OnLockPlayer;
        GameEvents.UnlockPlayer += OnUnlockPlayer;
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
        //FSM.UnlockStateMachine();
    }

    public void OnLockPlayer()
    {
        Locked = true;
        // FSM.LockStateMachine();
    }
}