

using System.Collections.Generic;
using Godot;

namespace GameRoot;

[Icon("res://components/behaviour/finite-state-machine/state_icon.png")]
[GlobalClass]
public partial class State : Node
{
    [Signal]
    public delegate void StateEnteredEventHandler();

    [Signal]
    public delegate void StateFinishedEventHandler(string nextState, Godot.Collections.Dictionary<string, Variant> parameters);

    public FiniteStateMachine FSM;
    public List<State> PreviousStates = new();
    public Godot.Collections.Dictionary<string, Variant> parameters = new();

    public virtual void Ready()
    {

    }

    public virtual void Enter()
    {

    }

    public virtual void Exit()
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