using System;
using Godot;
using Godot.Collections;

public partial class StateMachine : Node
{
    //Generic state machine. Initializes states and delegates engine callbacks
    //(_PhysicsProcess, _UnhandledInput) to the active state.

    //Emitted when transitioning to a new state.
    [Signal]
    public delegate void TransitionedEventHandler(string stateName);

    //Path to the initial active state. We export it to be able to pick the initial state in the inspector.
    [Export]
	public NodePath InitialState { get; set; } = new NodePath();

    //The previous state of the active state. At the start of the game is null.
    public State PreviousState { get; set; } = null;

    //The current active state. At the start of the game, we get the `initial_state`.
    private State currentState { get; set; }

	public override void _Ready()
	{
        currentState = (State) GetNode(InitialState);

        // Waiting for the owner to be ready
        WaitForOwnerReady();

        // The state machine assigns itself to the State objects' state_machine property.
        foreach (Node child in GetChildren())
        {
            if (child is State stateChild)
            {
                stateChild.StateMachine = this;
            }
        }

        currentState.Enter();
    }

    private async void WaitForOwnerReady()
    {
        // Waiting until the owner is ready
        await ToSignal(Owner, "ready");
    }

    // The state machine subscribes to node callbacks and delegates them to the state objects.
    public override void _UnhandledInput(InputEvent @event)
    {
        currentState.HandleInput(@event);
    }

    public override void _Process(double delta)
    {
        currentState.Update(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        currentState.PhysicsUpdate(delta);
    }

    // This function calls the current state's exit() function, then changes the active state,
    // and calls its enter function.
    // It optionally takes a `msg` dictionary to pass to the next state's enter() function.
    public void TransitionTo(string targetStateName, Dictionary msg = null)
    {
        // Safety check, you could use an assert() here to report an error if the state name is incorrect.
        // We don't use an assert here to help with code reuse. If you reuse a state in different state machines
        // but you don't want them all, they won't be able to transition to states that aren't in the scene tree.
        if (!HasNode(targetStateName))
            return;

        GD.Print($"targetStateName: {targetStateName}");

        currentState.Exit();
        PreviousState = currentState;
        currentState = GetNode<State>(targetStateName);
        currentState.Enter(msg);
        EmitSignal(nameof(Transitioned), currentState.Name);
    }
}
