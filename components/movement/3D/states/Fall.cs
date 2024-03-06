using System;
using Godot;

namespace GameRoot;

[GlobalClass]
public partial class Fall : Motion
{
    [Export] public float AirControlSpeed = 7f;
    [Export] public float Acceleration = 15f;
    [ExportGroup("CoyoteTime")]
    [Export] public bool CoyoteTime = true;
    [Export] public int CoyoteTimeFrames = 10;

    [ExportGroup("JumpBuffer")]
    [Export] public bool JumpBuffer = true;
    [Export] public int JumpBufferTimeFrames = 25; // This the time to record the jump buffer on 60 fps, so 15/60 will be 1/4s

    private int CurrentJumpBufferTimeFrames;
    private bool JumpRequested = false;
    private int CurrentCoyoteTimeFrames;
    public override void Enter()
    {
        JumpRequested = false;
        CurrentJumpBufferTimeFrames = JumpBufferTimeFrames;
        CurrentCoyoteTimeFrames = CoyoteTimeFrames;
    }

    public override void PhysicsUpdate(double delta)
    {
        base.PhysicsUpdate(delta);

        JumpRequested = Input.IsActionJustPressed("jump");
        CurrentCoyoteTimeFrames -= 1;
        CurrentJumpBufferTimeFrames -= 1;

        if (CoyoteTimeCountIsActive() && JumpRequested)
            FSM.ChangeStateTo("Jump");

        if ((!WasGrounded && IsGrounded) || Actor.IsOnFloor())
        {
            if (JumpBufferCountIsActive() && JumpRequested)
            {
                FSM.ChangeStateTo("Jump");
            }

            FSM.ChangeStateTo("Idle");
        }

        Move(AirControlSpeed, delta, Acceleration);

        Actor.MoveAndSlide();

        DetectWallRun();
    }

    private bool JumpBufferCountIsActive()
    {
        return JumpBuffer && Actor.Jump && CurrentJumpBufferTimeFrames > 0;
    }

    private bool CoyoteTimeCountIsActive()
    {
        return CoyoteTime && Actor.Jump && CurrentCoyoteTimeFrames > 0 && FSM.LastState() is not Jump; // This last check avoid doing unexpected double jump
    }
}