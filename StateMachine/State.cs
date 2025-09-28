using System;
using Godot;
using Godot.Collections;

//Virtual base class for all states.
public partial class State : Node
{
	//Reference to the state machine, to call its `TransitionTo()` method directly.
	//That's one unorthodox detail of our state implementation, as it adds a dependency between the
	//state and the state machine objects, but we found it to be most efficient for our needs.
	//The state machine node will set it.
	public virtual StateMachine StateMachine { get; set; } = null;


	//Virtual method. Receives events from the `_UnhandledInput()` callback.
	public virtual void HandleInput(InputEvent @event)
	{ }


	//Virtual method. Corresponds to the `_Process()` callback.
	public virtual void Update(double delta)
	{ }


	//Virtual method. Corresponds to the `_PhysicsProcess()` callback.
	public virtual void PhysicsUpdate(double delta)
	{ }


    //Virtual method. Called by the state machine upon changing the active state. The `msg` parameter
    //is a dictionary with arbitrary data the state can use to initialize itself.
    public virtual void Enter(Dictionary msg = null)
	{ }


	//Virtual method. Called by the state machine before changing the active state. Use this function
	//to clean up the state.
	public virtual void Exit()
	{ }
}
