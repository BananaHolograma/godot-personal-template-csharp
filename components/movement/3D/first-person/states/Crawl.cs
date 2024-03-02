using Godot;

namespace GameRoot;

[GlobalClass]
public partial class Crawl : Motion
{
    [Export]
    public float Speed = 1f;
    public AnimationPlayer AnimationPlayer;
    public ShapeCast3D CeilShapeDetector;
    public override void Ready()
    {
        AnimationPlayer = Actor.GetNode<AnimationPlayer>("AnimationPlayer");
        CeilShapeDetector = Actor.GetNode<ShapeCast3D>("CeilShapeDetector");

    }

    public override void Enter()
    {
        AnimationPlayer.Play("crawl");
    }

    public override void Exit(State _nextState)
    {
        AnimationPlayer.PlayBackwards("crawl");
    }

    public override void PhysicsUpdate(double delta)
    {
        base.PhysicsUpdate(delta);

        if (!Input.IsActionPressed("crawl") && !CeilShapeDetector.IsColliding())
        {
            FSM.ChangeStateTo("Crouch");
        }

        Move(Speed, delta);
        Actor.MoveAndSlide();
    }
}