# Unity Code Review

You are performing a structured Unity C# code review. Analyse the selected code (or the file currently open in the editor if no code is selected) against two standards:

1. **Code Style** — conventions from `UnityCodeStyleInstructions.md`
2. **Performance** — patterns from `UnityPerformanceOptimizationInstructions.md`

---

## How to conduct the review

Work through each category below. For every issue found, output:

- **Category** — which area it falls under (e.g. Naming, Allocation, Caching)
- **Severity** — `Critical` / `Warning` / `Suggestion`
- **Location** — class name, method name, or line reference
- **Issue** — concise description of the problem
- **Fix** — corrected code snippet or specific action to take

After all issues, output a **Summary** with total counts per severity and an overall verdict: `Pass`, `Pass with warnings`, or `Fail`.

---

## Code Style Checklist

### Naming Conventions

- [ ] Private instance fields use `m_` prefix → `private int m_health;`
- [ ] Static fields use `s_` prefix → `private static int s_instanceCount;`
- [ ] Constants and static readonly values use `k_` prefix → `private const int k_maxPlayers = 4;`
- [ ] Public properties are PascalCase, no prefix → `public int Health { get; private set; }`
- [ ] Methods are PascalCase → `private void UpdateHealth()`
- [ ] Interfaces are prefixed with `I` → `IHealthProvider`
- [ ] Enums and enum values are PascalCase
- [ ] Local variables and parameters are camelCase, no prefix

### Formatting

- [ ] Allman brace style — opening brace on its own line
- [ ] No trailing whitespace, consistent 4-space indent
- [ ] One blank line between members; two blank lines between major sections

### Class Organisation (top to bottom)

1. Constants and static readonly fields
2. Serialized fields (`[SerializeField]`)
3. Private fields
4. Properties
5. Unity lifecycle methods in canonical order: `Awake → OnEnable → Start → Update → FixedUpdate → LateUpdate → OnDisable → OnDestroy`
6. Public methods
7. Private methods

### Method Naming

- [ ] `Process` / `Handle` — for responding to events or input
- [ ] `Is` / `Has` / `Can` — for boolean queries
- [ ] Avoid vague names: `Do`, `Execute`, `Run`, `Logic`, `Manager` (unless warranted)

### SerializeField Usage

- [ ] Uses `[SerializeField] private` instead of `public` for Inspector-exposed fields
- [ ] Header attributes `[Header("...")]` used to group related fields
- [ ] Tooltip attributes `[Tooltip("...")]` on non-obvious fields

### MonoBehaviour Patterns

- [ ] `Awake` used for self-initialisation; `Start` for cross-object references
- [ ] Coroutines prefer `WaitForSeconds` cached in a field, not allocated per call
- [ ] `OnEnable` / `OnDisable` used for subscribing / unsubscribing events

### Miscellaneous Style

- [ ] No `public` fields on MonoBehaviours — always `[SerializeField] private`
- [ ] `var` used only when type is obvious from the right-hand side
- [ ] Regions (`#region`) avoided — prefer logical ordering instead
- [ ] String interpolation preferred over concatenation in non-hot paths

---

## Performance Checklist

### Allocation-Free Update Loops

- [ ] No `new` in `Update`, `FixedUpdate`, or `LateUpdate`
- [ ] No LINQ in hot paths (`Where`, `Select`, `FirstOrDefault`, etc.)
- [ ] No string concatenation or `string.Format` in Update — use cached strings or `StringBuilder`
- [ ] No `GetComponent<T>()` in Update — cache in `Awake`/`Start`
- [ ] No `FindObjectOfType` or `FindObjectsByType` in Update

### Caching

- [ ] Component references cached in a field: `m_rb = GetComponent<Rigidbody>()`
- [ ] `Camera.main` not called in Update — cache it (`m_mainCamera = Camera.main`)
- [ ] `transform` access in tight loops cached to a local variable
- [ ] `WaitForSeconds` instances cached as static or instance fields

### Collections

- [ ] `List<T>` pre-sized with capacity where count is known: `new List<T>(expectedCount)`
- [ ] Prefer index loops over `foreach` on `List<T>` in hot paths
- [ ] `Dictionary` lookups in Update use `TryGetValue`, not `ContainsKey` + indexer

### Object Pooling

- [ ] Frequently spawned/destroyed objects use `UnityEngine.Pool.ObjectPool<T>`
- [ ] Pool `Get` / `Release` called correctly (no double-release)

### Physics

- [ ] Non-allocating physics queries used: `Physics.RaycastNonAlloc`, `OverlapSphereNonAlloc`
- [ ] Results array pre-allocated and reused
- [ ] `Rigidbody` manipulation in `FixedUpdate`, not `Update`

### Burst / Jobs (if applicable)

- [ ] `[BurstCompile]` on `IJob` / `IJobFor` implementations
- [ ] No managed types (string, class references) in Burst-compiled structs
- [ ] `NativeArray` / `NativeList` disposed correctly (using `using` or `.Dispose()`)

### Anti-Pattern Detection

Flag any occurrence of:

| Anti-pattern | Why it's a problem |
|---|---|
| `GameObject.Find(...)` at runtime | O(n) scene search, expensive |
| `SendMessage` / `BroadcastMessage` | Slow reflection-based dispatch |
| `Resources.Load` at runtime | Blocks main thread, deprecated pattern |
| `Camera.main` in `Update` | Calls `FindObjectByTag` internally each time |
| Coroutine with `yield return null` tight state machine | Prefer Update-based state if running every frame |
| `Invoke` / `InvokeRepeating` for game logic | Opaque timing, not cleanly cancellable |

---

## Output Format

```
## Unity Code Review — [FileName or selection]

### Issues Found

| # | Severity | Category | Location | Issue | Fix |
|---|----------|----------|----------|-------|-----|
| 1 | Critical  | Naming   | PlayerController.Update | Field `health` missing `m_` prefix | Rename to `m_health` |
| 2 | Warning   | Allocation | EnemyAI.Update | `GetComponent<Renderer>()` called each frame | Cache in `Awake`: `m_renderer = GetComponent<Renderer>()` |

### Summary

- Critical: X
- Warning: Y
- Suggestion: Z

**Verdict: [Pass / Pass with warnings / Fail]**
```

---

## Usage

**Claude Code** — invoke as a project skill:
```
/project:UnityCodeReview
```
Then reference the file: `Review @Assets/Scripts/PlayerController.cs`

**GitHub Copilot Chat** — reference this file and ask:
```
Using @UnityCodeReview.md, review the open file for style and performance issues.
```

**Any chat interface** — paste the checklist sections above along with your code and ask the model to work through them systematically.
