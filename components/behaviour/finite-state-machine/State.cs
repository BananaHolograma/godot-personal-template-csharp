using Godot;

namespace GameRoot;

[Icon("res://components/behaviour/finite-state-machine/state_icon.png")]
[GlobalClass]
public partial class State : Node
{
    [Signal]
    public delegate void StateEnteredEventHandler();

    [Signal]
    public delegate void StateFinishedEventHandler(string nextState);

    public FiniteStateMachine FSM;

    public virtual void Ready()
    {

    }

    public virtual void Enter()
    {

    }

    public virtual void Exit(State nextState)
    {

    }

    public virtual void HandleInput(InputEvent @event)
    {

    }

    public virtual void PhysicsUpdate(double delta)
    {

    }

    public virtual void Update(double delta)
    {

    }

    public virtual void OnAnimationPlayerFinished(string Name)
    {

    }

    public virtual void OnAnimationFinished()
    {

    }
}