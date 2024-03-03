using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExtensions;

namespace GameRoot;

[Icon("res://components/behaviour/finite-state-machine/icon.png")]
[GlobalClass]
public partial class FiniteStateMachine : Node
{
    #region Signals
    [Signal]
    public delegate void StateChangedEventHandler(State from, State to);
    [Signal]
    public delegate void StateChangeFailedEventHandler(State from, State to);
    [Signal]
    public delegate void StackPushedEventHandler(State newState, Godot.Collections.Array<State> stack);
    [Signal]
    public delegate void StackFlushedEventHandler(Godot.Collections.Array<State> stack);

    #endregion

    #region Exports
    [Export] public State CurrentState;
    [Export] public bool EnableStack = true;
    [Export] public int StackCapacity = 3;
    [Export] public bool FlushStackWhenReachCapacity = false;

    #endregion

    public readonly Dictionary<string, State> States = new();
    public readonly Dictionary<string, Transition> Transitions = new();
    public readonly Godot.Collections.Array<State> StatesStack = new();
    public bool Locked = false;

    #region Public
    public override void _Ready()
    {
        StateChanged += OnStateChanged;

        InitializeStateNodes();
        RegisterTransitions();

        if (CurrentState == null)
        {
            GD.PushError("This Finite state machine does not have an initial state defined");
            return;
        }

        EnterState(CurrentState);
    }

    public void ChangeStateTo(State nextState)
    {
        if (!StateExists(nextState))
        {
            GD.PushError($"The change of state cannot be done because the state {nextState} does not exits in this Finite State Machine");
            return;
        }

        if (CurrentStateIs(nextState))
            return;

        if (CurrentState is not null)
            RunTransition(CurrentState, nextState);

    }

    public void ChangeStateTo(string nextState)
    {
        if (!StateExists(nextState))
        {
            GD.PushError($"The change of state cannot be done because the state {nextState} does not exits in this Finite State Machine");
            return;
        }

        if (CurrentStateIs(nextState))
            return;

        if (CurrentState is not null)
            RunTransition(CurrentState, GetStateByName(nextState));

    }

    public void RunTransition(State from, State to)
    {
        string transitionName = BuildTransitionName(CurrentState, to);

        if (!Transitions.ContainsKey(transitionName))
            Transitions[transitionName] = new NeutralTransition();

        Transition transition = Transitions[transitionName];
        transition.FromState = CurrentState;
        transition.ToState = to;

        if (transition.ShouldTransition())
        {
            transition.OnTransition();
            EmitSignal(SignalName.StateChanged, transition.FromState, transition.ToState);
            return;
        }

        EmitSignal(SignalName.StateChangeFailed, transition.FromState, transition.ToState);
    }

    public bool CurrentStateIs(string name)
    {
        return name.Trim().ToLower().Equals(CurrentState.Name.ToString().Trim().ToLower());
    }

    public bool CurrentStateIs(State state)
    {
        return state == CurrentState;
    }

    public bool CurrentStateIsNot(string[] states)
    {
        return !states.Any(state => CurrentStateIs(state));
    }

    public bool StateExists(State state)
    {
        return States.ContainsKey(state.Name);
    }
    public bool StateExists(string name)
    {
        return States.ContainsKey(name);
    }


    public void EnterState(State state)
    {
        state.Enter();
    }

    public void ExitState(State state, State nextState)
    {
        state.Exit(nextState);
    }

    public State GetStateByName(string name)
    {
        if (States.ContainsKey(name))
            return States[name];

        return null;
    }

    public void PushStateToStack(State state)
    {
        if (EnableStack && StackCapacity > 0)
        {
            if (StatesStack.Count >= StackCapacity)
            {
                if (FlushStackWhenReachCapacity)
                {
                    StatesStack.Clear();
                    EmitSignal(SignalName.StackFlushed, StatesStack);
                }
                else
                {
                    StatesStack.RemoveAt(0);
                }
            }

            StatesStack.Add(state);

            EmitSignal(SignalName.StackPushed, state, StatesStack);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        CurrentState.HandleInput(@event);
    }

    public override void _PhysicsProcess(double delta)
    {
        CurrentState.PhysicsUpdate(delta);
    }

    public override void _Process(double delta)
    {
        CurrentState.Update(delta);
    }

    public void LockStateMachine()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);
        SetProcessUnhandledInput(false);
    }

    public void UnlockStateMachine()
    {
        SetProcess(true);
        SetPhysicsProcess(true);
        SetProcessInput(true);
        SetProcessUnhandledInput(true);
    }

    #endregion

    #region PrivateFunctions
    private void InitializeStateNodes(Node node = null)
    {
        List<State> states = new();
        this.FindNodeClassesRecursively<State>(states);

        foreach (State state in states)
        {
            AddStateToDictionary(state);
        }
    }
    /// <summary>
    /// To register a new transition just use like this example: Transitions.Add("IdleToWalk", new NeutralTransition());
    /// </summary>
    private void RegisterTransitions()
    {
        Transitions["WalkToRun"] = new WalkToRunTransition();
        Transitions["RunToWalk"] = new RunToWalkTransition();
        Transitions["JumpToWallRun"] = new JumpToWallRunTransition();

    }

    private string BuildTransitionName(State from, State to) => $"{from.Name.ToString().Trim()}To{to.Name.ToString().Trim()}";

    private void AddStateToDictionary(State state)
    {
        States.Add(state.Name, state);
        state.FSM = this;
        state.Ready();
    }

    #endregion

    #region SignalCallbacks
    public void OnStateChanged(State from, State to)
    {
        PushStateToStack(from);
        ExitState(from, to);
        EnterState(to);

        CurrentState = to;

    }

    #endregion
}
