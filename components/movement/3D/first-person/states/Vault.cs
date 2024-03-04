using Godot;

namespace GameRoot;

[GlobalClass]
public partial class Vault : Motion
{
    [Export] float Speed = 5f;
    public Vector3 MovementAmount = Vector3.Zero;
    public Vector3 TargetPosition = Vector3.Zero;

    public override void Enter()
    {
        TargetPosition = Actor.GlobalPosition + MovementAmount;

        GD.Print("Movement amount and target ", Actor.GlobalPosition, MovementAmount, TargetPosition);
    }

    public override void Exit(State _nextState)
    {
        MovementAmount = Vector3.Zero;
        TargetPosition = Vector3.Zero;
    }

    public override void PhysicsUpdate(double delta)
    {
        if (Actor.GlobalPosition.DistanceTo(TargetPosition) <= Mathf.Epsilon)
        {
            FSM.ChangeStateTo("Walk");
        }

        Actor.GlobalPosition = Actor.GlobalPosition.MoveToward(TargetPosition, (float)(Speed * delta));
    }
}