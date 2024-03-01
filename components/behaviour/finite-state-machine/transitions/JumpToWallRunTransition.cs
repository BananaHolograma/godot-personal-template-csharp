namespace GameRoot;

public class JumpToWallRunTransition : Transition
{
    public override bool ShouldTransition() => true;
    public override void OnTransition()
    {
        if (FromState is Jump jump && ToState is WallRun wallRun)
            wallRun.WallNormals = new(jump.WallNormals);
    }
}