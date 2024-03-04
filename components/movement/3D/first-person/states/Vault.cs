using Godot;

namespace GameRoot;

[GlobalClass]
public partial class Vault : Motion
{
    [Export] float Speed = 2.5f;
    [Export] float VaultTilt = 7f;
    public Vector3 MovementAmount = Vector3.Zero;
    public Vector3 TargetPosition = Vector3.Zero;

    public Node3D Head;
    private RandomNumberGenerator rng = new();

    public override void Ready()
    {
        Head = Actor.GetNode<Node3D>("Head");
    }

    public override void Enter()
    {
        Vector3 originalHeadPosition = Head.Position;

        Tween tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(Head, "rotation:z", (rng.Randf() <= .5f ? 1 : -1) * Mathf.DegToRad(VaultTilt), .2f).SetEase(Tween.EaseType.In);
        tween.TweenProperty(Head, "position:y", Head.Position.Y + .5f, .25f).SetEase(Tween.EaseType.Out);
        tween.Chain().TweenProperty(Head, "position", originalHeadPosition, .2f).SetEase(Tween.EaseType.Out);

        TargetPosition = Actor.GlobalPosition + MovementAmount;
    }

    public override void Exit(State _nextState)
    {
        MovementAmount = Vector3.Zero;
        TargetPosition = Vector3.Zero;

        Tween tween = CreateTween();
        tween.TweenProperty(Head, "rotation:z", 0, .5f).SetEase(Tween.EaseType.Out);
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