namespace GameRoot;

public class WallRunToJumpTransition : Transition
{
    public override bool ShouldTransition() => true;
    public override void OnTransition()
    {
        if (FromState is WallRun wallRun && ToState is Jump jump)
        {
            jump.JumpHorizontalBoost = wallRun.JumpHorizontalBoost;
            jump.JumpVerticalBoost = wallRun.JumpVerticalBoost;

        }
    }
}