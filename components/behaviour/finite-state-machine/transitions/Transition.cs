
namespace GameRoot;

public abstract class Transition
{
	public State FromState { get; set; }
	public State ToState { get; set; }

	public abstract bool ShouldTransition();
	public abstract void OnTransition();
}
