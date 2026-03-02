# Unity Best Practices — Quick Reference

## Field Naming Prefixes

| Prefix | Use |
|---|---|
| `m_` | Private instance fields (`private int m_Health;`) |
| `s_` | Static fields (`private static int s_Count;`) |
| `k_` | Constants (`private const int k_MaxHealth = 100;`) |
| _(none)_ | Properties, public fields, parameters, local vars |

## Class Member Order

1. Using statements
2. Namespace declaration
3. Fields (serialized → private → static → constants)
4. Properties
5. Events
6. MonoBehaviour methods (Awake → OnEnable → Start → OnDisable → OnDestroy → FixedUpdate → Update → LateUpdate)
7. Public methods
8. Private methods
9. Nested classes / structs

## Method Naming

| Pattern | Purpose | Example |
|---|---|---|
| `ProcessX` | Game logic computation | `ProcessDamage()` |
| `HandleX` | Event callback | `HandlePlayerDied()` |
| `IsX` / `HasX` / `CanX` | Boolean query | `IsAlive()`, `CanJump()` |
| `SetX` / `GetX` | Explicit accessor | `SetHealth(int)` |

## Event Conventions

- Past-tense names: `PlayerDied`, `ScoreChanged`, `LevelLoaded`
- Subscribe in `OnEnable`, unsubscribe in `OnDisable`
- Static events for global bus: `static event Action<T> EventName`

## Braces — Allman Style

```csharp
// Correct
if (condition)
{
    DoSomething();
}

// Wrong
if (condition) {
    DoSomething();
}
```

## Performance — Hot Path Rules

- Never allocate in `Update` / `FixedUpdate` / `LateUpdate`
- Cache all component refs in `Awake` (never call `GetComponent` in Update)
- Use `CompareTag("Tag")` not `gameObject.tag == "Tag"` (string alloc)
- Use `TryGetComponent<T>()` not `GetComponent<T>()` (avoids exception on miss)
- Prefer `ObjectPool<T>` over Instantiate/Destroy for frequent objects
- Cache `Animator` param hashes: `private static readonly int k_SpeedHash = Animator.StringToHash("Speed");`
- Use `MaterialPropertyBlock` for per-instance shader values (not `material.SetX`)

## UI Toolkit — UXML/USS BEM Naming

```
block__element--modifier
```
- Block: `inventory-panel`
- Element: `inventory-panel__slot`
- Modifier: `inventory-panel__slot--selected`
- CSS variables: `--color-primary`, `--spacing-md`

## Unity 6 Async — Awaitable

```csharp
// Unity 6+
await Awaitable.WaitForSecondsAsync(delay, cancellationToken);

// Guard after every await:
if (this == null || !isActiveAndEnabled) return;
```

## Assembly Definitions

- One `.asmdef` per logical layer: `MyGame.Runtime`, `MyGame.Editor`, `MyGame.Tests`
- Editor assemblies: `"includePlatforms": ["Editor"]`
- Test assemblies: `"testPlatforms": ["EditMode", "PlayMode"]`
- `rootNamespace` must match the `.asmdef` name
