using Godot;
using GodotExtensions;

namespace GameRoot;

[GlobalClass]
public partial class Idle : Motion
{
    [Export] public float Friction = 7f;

    public override void Enter()
    {
        Actor.Velocity = Vector3.Zero;
        FrontWallDetector.Enabled = true;
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