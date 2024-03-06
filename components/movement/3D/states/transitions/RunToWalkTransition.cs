namespace GameRoot;


public class RunToWalkTransition : Transition
{
    public override bool ShouldTransition() => true;
    public override void OnTransition()
    {
        if (FromState is Run run && ToState is Walk walk && run.InRecovery)
            walk.CatchingBreathTimer.Start();
    }
}