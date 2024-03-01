using Godot;
using GodotExtensions;

namespace GameRoot;

[GlobalClass]
public partial class Idle : Motion
{
    public override void Enter()
    {
        Actor.Velocity = Vector3.Zero;
    }

    public override void PhysicsUpdate(double delta)
    {
        base.PhysicsUpdate(delta);

        if (TransformedInput.InputDirection.IsNotZeroApprox())
        {
            FSM.ChangeStateTo("Walk");
            return;
        }

        Actor.Velocity = Actor.Velocity.Lerp(Vector3.Zero, (float)delta * Friction);
        Actor.MoveAndSlide();

        DetectCrouch();
        DetectJump();
    }
}