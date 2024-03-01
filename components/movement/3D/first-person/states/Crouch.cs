using System.Linq;
using Godot;

namespace GameRoot;

[GlobalClass]
public partial class Crouch : Motion
{
    [Export]
    public float Speed = 2f;
    public AnimationPlayer AnimationPlayer;

    public override void Ready()
    {
        AnimationPlayer = Actor.GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void Enter()
    {
        bool lastStateWasNotCrawlingOrSliding = !new[] { "Crawl", "Slide" }.Any(stateName => stateName == FSM.StatesStack.Last().Name);

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

        if (!Input.IsActionPressed("crouch") && !Actor.CeilShapeCast.IsColliding())
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

        Actor.MoveAndSlide();

        DetectCrawl();
    }

}