
namespace GameRoot;

public abstract class Transition
{
	public readonly FiniteStateMachine FSM;
	public State FromState { get; set; }
	public State ToState { get; set; }

	public abstract bool ShouldTransition();
	public abstract void OnTransition();
}
