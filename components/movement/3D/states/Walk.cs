using System.Linq;
using Godot;

namespace GameRoot;

[GlobalClass]
public partial class Walk : Motion
{
    [Export] public float Speed = 3.5f;
    [Export] public float Acceleration = 10f;
    [Export] public float Friction = 0;
    [Export] public float CatchingBreathRecoveryTime = 3f;
    public Timer CatchingBreathTimer;

    public override void Ready()
    {
        CreateCatchingBreathTimer();
    }
    public override void Enter()
    {
        FootstepManager.FloorDetectorRaycast.Enabled = true;
        FrontWallDetector.Enabled = true;
        Actor.Velocity = Actor.Velocity with { Y = 0 };
    }

    public override void Exit(State _nextState)
    {
        FootstepManager.FloorDetectorRaycast.Enabled = false;
    }

    public override void PhysicsUpdate(double delta)
    {
        base.PhysicsUpdate(delta);

        Move(Speed, delta, Acceleration);

        if (TransformedInput.WorldCoordinateSpaceDirection.IsZeroApprox() || Actor.Velocity.IsZeroApprox())
            FSM.ChangeStateTo("Idle");

        Actor.GetNode<FootstepManager>("FootstepManager").Footstep(this);

        if (Input.IsActionPressed("run"))
            FSM.ChangeStateTo("Run");

        StairStepUp();

        Actor.MoveAndSlide();

        StairStepDown();
        DetectCrouch();
        LedgeDetect();
        DetectJump();
    }

    private void CatchingBreath()
    {
        if (IsInstanceValid(CatchingBreathTimer) && FSM.StatesStack.Count > 0)
        {
            if (FSM.StatesStack.Last() is Run runState && runState.InRecovery)
                CatchingBreathTimer.Start();
        }
    }

    private void CreateCatchingBreathTimer()
    {
        if (CatchingBreathTimer == null)
        {
            CatchingBreathTimer = new Timer
            {
                Name = "RunCatchingBreathTimer",
                WaitTime = CatchingBreathRecoveryTime,
                ProcessCallback = Timer.TimerProcessCallback.Physics,
                Autostart = false,
                OneShot = true
            };

            AddChild(CatchingBreathTimer);
        }
    }
}