using System.Linq;
using Godot;

namespace GameRoot;

[GlobalClass]
public partial class Walk : Motion
{
    [Export]
    public float Speed = 3.5f;
    [Export]
    public float CatchingBreathRecoveryTime = 3f;

    public Timer CatchingBreathTimer;


    public override void Ready()
    {
        CreateCatchingBreathTimer();
    }
    public override void Enter()
    {
        // CatchingBreath();
        Actor.Velocity = Actor.Velocity with { Y = 0 };
        GD.Print("Entro walk");
    }

    public override void PhysicsUpdate(double delta)
    {
        base.PhysicsUpdate(delta);

        Move(Speed, delta);

        if (TransformedInput.WorldCoordinateSpaceDirection.IsZeroApprox() || Actor.Velocity.IsZeroApprox())
            FSM.ChangeStateTo("Idle");


        if (Input.IsActionPressed("run"))
            FSM.ChangeStateTo("Run");

        Actor.MoveAndSlide();

        DetectCrouch();
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