namespace GameRoot;


public class WalkToRunTransition : Transition
{
    public override bool ShouldTransition()
    {
        if (FromState is Walk walk && ToState is Run _run)
            return walk.CatchingBreathTimer.IsStopped() && walk.Actor.Run;

        return true;
    }
    public override void OnTransition() { }
}