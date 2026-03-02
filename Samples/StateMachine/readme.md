# Sample: State Machine

Demonstrates a simple, reusable state machine pattern using the `IState` interface and a generic `StateMachine` class. States are responsible for their own enter/update/exit logic.

## How it works

- `IState` defines the contract: `OnEnter()`, `OnUpdate()`, `OnExit()`
- `StateMachine` holds the current state, calls `OnExit()` on the old state and `OnEnter()` on the new state during transitions
- `ExampleStateMachineUser` owns the state machine and concrete states (as nested classes). It calls `m_StateMachine.Update()` from its `Update()` method

## Setup

1. Add `ExampleStateMachineUser` to any GameObject in your scene
2. Press Play
3. Press **Space** to toggle between Idle and Moving states — watch the Console for state transition logs

## Extending it

- Create your own state classes implementing `IState`
- Store them in the owner MonoBehaviour and call `m_StateMachine.ChangeState(myNewState)` when conditions are met
- States can reference the owner via a constructor parameter (as shown with `ExampleStateMachineUser owner`)

## Key takeaways

- State logic is fully encapsulated — no giant switch statements in Update()
- Clean entry/exit lifecycle prevents state bleed
- Nested private classes keep related state logic co-located with the owner
