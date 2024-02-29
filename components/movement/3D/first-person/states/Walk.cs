using Godot;

namespace GameRoot;

[GlobalClass]
public partial class Walk : Motion
{

    [Export]
    public float Speed = 3.5f;

    public override void Enter()
    {
        Actor.Velocity = Actor.Velocity with { Y = 0 };
        GD.Print("ENTER WALK ", Actor.Velocity);
    }

    public override void PhysicsUpdate(double delta)
    {
        base.PhysicsUpdate(delta);

        Move(Speed, delta);

        if (TransformedInput.WorldCoordinateSpaceDirection.IsZeroApprox() || Actor.Velocity.IsZeroApprox())
        {
            FSM.ChangeStateTo("Idle");
            return;
        }

        Actor.MoveAndSlide();
    }
}