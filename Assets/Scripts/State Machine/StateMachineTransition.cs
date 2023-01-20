using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


internal class StateMachineTransition
{
    /// <summary>
    /// Tests that the condition(s) required by this transition are met.
    /// </summary>
    public Func<bool> Condition { get; }

    /// <summary>
    /// The state this transition leads into.
    /// </summary>
    public IStateMachineState DestinationState { get; }


    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="destinationState">The state this transition leads into.</param>
    /// <param name="condition">A delegate that tests that the condition(s) required by this transition are met.</param>
    public StateMachineTransition(IStateMachineState destinationState, Func<bool> condition)
    {
        DestinationState = destinationState;
        Condition = condition;
    }

}
