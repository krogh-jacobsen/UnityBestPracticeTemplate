# Sample: ScriptableObject Event System

Demonstrates the Observer pattern using ScriptableObject event channels — a common Unity architecture that decouples senders from receivers without requiring direct references.

## How it works

- `GameEvent` is a ScriptableObject asset. Any script can call `myEvent.Raise()`.
- `GameEventListener` is a MonoBehaviour. It subscribes to a `GameEvent` asset and invokes a `UnityEvent` when raised.
- Senders and listeners never reference each other — only the shared `GameEvent` asset.

## Setup

1. Create a `GameEvent` asset: **Assets > Create > Best Practices > Game Event**
2. Add a `GameEventListener` to any GameObject
3. Assign the `GameEvent` asset to the listener's **Event** field
4. Add responses to the listener's **Response** UnityEvent field
5. Call `myEvent.Raise()` from any script to trigger all listeners

## Key takeaways

- No singleton or service locator required
- Works across scenes (both objects reference the same SO asset)
- Subscribe/Unsubscribe in OnEnable/OnDisable — automatically safe
