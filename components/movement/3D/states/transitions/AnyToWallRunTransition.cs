using Godot;

namespace GameRoot;

public class AnyToWallRunTransition : Transition
{
    public override bool ShouldTransition() => true;
    public override void OnTransition()
    {
        if (FromState is Motion from && ToState is WallRun wallRun)
        {
            wallRun.WallNormals = new(from.WallNormals);
            //from.WallNormals = new();
        }
    }
}