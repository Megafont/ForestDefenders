using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;



public class StateMachine
{
    // The current state the machine is in.
    private IStateMachineState _CurrentState;

    // A dictionary containing a list of state lists. Each list contains a list of transitions that must begin from a specified state.
    private Dictionary<Type, List<StateMachineTransition>> _Transitions = new Dictionary<Type, List<StateMachineTransition>>();
    // A list of transitions that must begin from the current state.
    private List<StateMachineTransition> _CurrentTransitions = new List<StateMachineTransition>();
    // A list of transitions that can begin from any other state.
    private List<StateMachineTransition> _FromAnyStateTransitions = new List<StateMachineTransition>();

    // An empty list used to set _CurrentTransitions to an empty list every time a check returns null from the _Transitions dictionary in SetState().
    // This prevents allocating a new list every time that happens.
    private static List<StateMachineTransition> EmptyTransitions = new List<StateMachineTransition>(0);



    /// <summary>
    /// Called to update the state machine. This 
    /// </summary>
    public void Tick()
    {
        // Check if we should change states.
        StateMachineTransition transition = GetTransition();

        if (transition != null)
            SetState(transition.DestinationState);


        // Tell the current state to run it's logic.
        _CurrentState?.Tick();
    }

    /// <summary>
    /// Sets the current state of the state machine.
    /// </summary>
    /// <param name="newState">The state to set the state machine to.</param>
    public void SetState(IStateMachineState newState)
    {
        if (newState == null)
        {
            Debug.LogError("Could not change states. The passed in state is null!");
            return;
        }
        
        if (newState == _CurrentState)
            return;


        _CurrentState?.OnExit();
        _CurrentState = newState;

        _Transitions.TryGetValue(_CurrentState.GetType(), out _CurrentTransitions);
        if (_CurrentTransitions == null)
            _CurrentTransitions = EmptyTransitions;

        _CurrentState?.OnEnter();
    }

    /// <summary>
    /// Adds a transition to the specified state that can start from any other state.
    /// </summary>
    /// <param name="destinationState">The state this transition leads to.</param>
    /// <param name="condition">A delegate that tests that the condition(s) required by this transition are met.</param>
    /// <exception cref="ArgumentNullException">If any of the parameters are null.</exception>
    public void AddTransitionFromAny(IStateMachineState destinationState, Func<bool> condition)
    {
        if (destinationState == null)
            throw new Exception("Could not add transition from state. The passed in destination state is null!");
        if (condition == null)
            throw new Exception("Could not add transition from state. The passed in condition delegate is null!");


        _FromAnyStateTransitions.Add(new StateMachineTransition(destinationState, condition));
    }

    /// <summary>
    /// Adds a transition to the specified state that must start from the specific state specified.
    /// </summary>
    /// <param name="previousState">The state we must currently be in for this transition to be valid.</param>
    /// <param name="destinationState">The state this transition leads to.</param>
    /// <param name="condition">A delegate that tests that the condition(s) required by this transition are met.</param>
    /// <exception cref="ArgumentNullException">If any of the parameters are null.</exception>
    public void AddTransitionFrom(IStateMachineState previousState, IStateMachineState destinationState, Func<bool> condition)
    {
        if (previousState == null)
            throw new ArgumentNullException("Could not add transition from state. The passed in previous state is null!");
        if (destinationState == null)
            throw new ArgumentNullException("Could not add transition from state. The passed in destination state is null!");
        if (condition == null)
            throw new ArgumentNullException("Could not add transition from state. The passed in condition delegate is null!");


        if (_Transitions.TryGetValue(previousState.GetType(), out List<StateMachineTransition> transitions) == false)
        {
            transitions = new List<StateMachineTransition>();
            _Transitions[previousState.GetType()] = transitions;
        }

        transitions.Add(new StateMachineTransition(destinationState, condition));
    }

    private StateMachineTransition GetTransition()
    {
        // First, look for a transition that can start from any other state.
        foreach (StateMachineTransition transition in _FromAnyStateTransitions)
        {
            if (transition.Condition())
                return transition;
        }


        // We did not find a valid transition that can start from any other state.
        // So now look for one that can only come from the current state.
        foreach (StateMachineTransition transition in _CurrentTransitions)
        {
            if (transition.Condition())
                return transition;
        }


        // No valid transition was found.
        return null;
    }


}
