using Godot;

namespace GameRoot;

[GlobalClass]
public partial class Fall : Motion
{
    [Export]
    public bool JumpBuffer = true;
    [Export]

    public int JumpBufferTimeFrames = 15; // This the time to record the jump buffer on 60 fps, so 15/60 will be 1/4s
    public int CurrentJumpBufferTimeFrames = 15;

    public bool JumpRequested = false;
    public override void Enter()
    {
        JumpRequested = false;
        CurrentJumpBufferTimeFrames = JumpBufferTimeFrames;
    }

    public override void PhysicsUpdate(double delta)
    {
        base.PhysicsUpdate(delta);

        JumpRequested = Input.IsActionJustPressed("jump");

        if (JumpBufferCountIsActive())
            CurrentJumpBufferTimeFrames -= 1;

        if (!WasGrounded && IsGrounded)
        {
            if (JumpBufferCountIsActive() && JumpRequested)
            {
                CurrentJumpBufferTimeFrames = 0;
                FSM.ChangeStateTo("Jump");
                return;
            }

            FSM.ChangeStateTo("Idle");

        }

        Move(2f, delta);

        Actor.MoveAndSlide();
    }

    private bool JumpBufferCountIsActive()
    {
        return JumpBuffer && Actor.Jump && CurrentJumpBufferTimeFrames > 0;
    }
}