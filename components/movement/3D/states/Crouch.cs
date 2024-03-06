using System.Linq;
using Godot;

namespace GameRoot;

[GlobalClass]
public partial class Crouch : Motion
{
    [Export] public float Speed = 2f;
    public AnimationPlayer AnimationPlayer;
    public ShapeCast3D CeilShapeDetector;

    public override void Ready()
    {
        AnimationPlayer = Actor.GetNode<AnimationPlayer>("AnimationPlayer");
        CeilShapeDetector = Actor.GetNode<ShapeCast3D>("CeilShapeDetector");
    }

    public override void Enter()
    {
        bool lastStateWasNotCrawlingOrSliding = !new[] { "Crawl", "Slide" }.Any(stateName => stateName == FSM.LastState()?.Name);

        if (FSM.StatesStack.Count > 0 && lastStateWasNotCrawlingOrSliding)
            AnimationPlayer.Play("crouch");
    }

    public override void Exit(State nextState)
    {
        if (nextState is not Crawl)
            AnimationPlayer.PlayBackwards("crouch");
    }

    public override void PhysicsUpdate(double delta)
    {
        base.PhysicsUpdate(delta);

        if (!Input.IsActionPressed("crouch") && !CeilShapeDetector.IsColliding())
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

        Move(Speed, delta);

        StairStepUp();

        Actor.MoveAndSlide();

        StairStepDown();

        DetectCrawl();
    }

}