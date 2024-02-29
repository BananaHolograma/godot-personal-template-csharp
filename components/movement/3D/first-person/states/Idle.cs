using Godot;

namespace GameRoot;

[GlobalClass]
public partial class Idle : Motion
{

    public override void Enter()
    {
        if (Actor is not null)
        {
            Actor.Velocity = Vector3.Zero;

        }
    }

    public override void PhysicsUpdate(double delta)
    {
        base.PhysicsUpdate(delta);

        /*  if (!TransformedInput.InputDirection.IsZeroApprox())
         {
             EmitSignal(SignalName.StateFinished, "Walk", new());
         } */

        Actor.Velocity = Actor.Velocity.Lerp(Vector3.Zero, (float)delta * Friction);
        Actor.MoveAndSlide();
    }
}