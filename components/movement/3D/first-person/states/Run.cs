using System;
using Godot;

namespace GameRoot;

[GlobalClass]
public partial class Run : Motion
{
    [Export] public float Speed = 5.5f;
    [Export] public float Acceleration = 25f;
    [Export] public float Friction = 0;
    [Export] public float SprintTime = 4f;


    public Timer SpeedTimer;

    public bool InRecovery = false;

    public override void Ready()
    {
        CreateSpeedTimer();
    }

    public override void Enter()
    {
        if (SprintTime > 0 && IsInstanceValid(SpeedTimer))
            SpeedTimer.Start();

        InRecovery = false;
        FootstepManager.FloorDetectorRaycast.Enabled = true;
    }

    public override void Exit(State _nextState)
    {
        SpeedTimer.Stop();
        FootstepManager.FloorDetectorRaycast.Enabled = false;
    }

    public override void PhysicsUpdate(double delta)
    {
        base.PhysicsUpdate(delta);

        Move(Speed, delta, Acceleration);

        if (Input.IsActionJustReleased("run"))
        {
            if (Actor.Velocity.IsZeroApprox())
            {
                FSM.ChangeStateTo("Idle");
            }
            else
            {
                FSM.ChangeStateTo("Walk");
            }
        }

        if (Input.IsActionJustPressed("crouch") && Actor.Slide)
            FSM.ChangeStateTo("Slide");

        StairStepUp();

        Actor.MoveAndSlide();

        StairStepDown();
        LedgeDetect();
        DetectJump();
    }

    private void CreateSpeedTimer()
    {
        if (SpeedTimer == null)
        {
            SpeedTimer = new Timer
            {
                Name = "RunSpeedTimer",
                WaitTime = SprintTime,
                ProcessCallback = Timer.TimerProcessCallback.Physics,
                Autostart = false,
                OneShot = true
            };

            AddChild(SpeedTimer);
            SpeedTimer.Timeout += OnSpeedTimerTimeout;
        }
    }

    private void OnSpeedTimerTimeout()
    {
        InRecovery = true;
        FSM.ChangeStateTo("Walk");
    }
}