using Godot;

namespace GameRoot;

[GlobalClass]
public partial class Fall : Motion
{
    public override void PhysicsUpdate(double delta)
    {
        base.PhysicsUpdate(delta);

        if (!WasGrounded && IsGrounded)
        {
            FSM.ChangeStateTo("Idle");
        }

        Move(2f, delta);

        Actor.MoveAndSlide();
    }
}