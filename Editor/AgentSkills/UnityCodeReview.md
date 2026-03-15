# Unity Code Review

You are performing a structured Unity code review. The input may be:

- A **single file** (selected code or the file currently open in the editor)
- A **folder path** — in which case recursively find and review **all** `.cs`, `.uss`, and `.uxml` files within it and its subfolders

Apply the correct checklist based on file type:

| File type | Standards to apply |
|---|---|
| `.cs` | Code Style (`UnityCodeStyleInstructions.md`) + Performance (`UnityPerformanceOptimizationInstructions.md`) |
| `.uss` / `.uxml` | UI Toolkit (`UnityUIToolkitInstructions.md`) — BEM naming, USS/UXML conventions, data binding patterns |

When reviewing a folder, process each file in turn and produce a combined report with per-file sections.

---

## How to conduct the review

Work through each category below. For every issue found, output:

- **Category** — which area it falls under (e.g. Naming, Allocation, BEM, USS Property)
- **Severity** — `Critical` / `Warning` / `Suggestion`
- **Location** — file name, class/method name, or element name/line reference
- **Issue** — concise description of the problem
- **Fix** — corrected code/markup snippet or specific action to take

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

## UI Toolkit Checklist (.uss / .uxml)

### BEM Naming Conventions

- [ ] Class names use **kebab-case** — `navbar-menu`, not `navbarMenu` or `NavbarMenu`
- [ ] BEM blocks are meaningful standalone components — `login-form`, not generic `form`
- [ ] BEM elements use `__` separator — `login-form__input-field`, not `login-form-input`
- [ ] BEM modifiers use `--` separator — `button--primary`, not `button-primary`
- [ ] No camelCase, PascalCase, or underscores in class/name values

### USS Properties

- [ ] Colors use `rgb()` or `rgba()` — **no hex values** (`#FF6432` does not work in USS)
- [ ] No `calc()` — not supported in USS; use `flex-grow` or fixed values instead
- [ ] No `display: grid` — USS supports **flexbox only**
- [ ] No `gap` property — use `margin` on child elements instead
- [ ] No `em` / `rem` / `vw` / `vh` units — only `px` and `%`
- [ ] Text alignment uses `-unity-text-align`, not `text-align`
- [ ] Font style uses `-unity-font-style`, not `font-style`
- [ ] No `:nth-child()`, `:not()`, `:first-child`, `:last-child` — unsupported pseudo-classes
- [ ] CSS variables defined in `:root {}` only — not scoped to other selectors

### UXML Structure

- [ ] File named in **PascalCase** — `MainMenu.uxml`, not `main-menu.uxml`
- [ ] Elements use `name` for unique C# query targets (kebab-case)
- [ ] Elements use `class` for reusable BEM styles
- [ ] `binding-path` values exactly match the C# property name (case-sensitive)
- [ ] `[CreateProperty]` attribute present on all bound C# properties
- [ ] `data-source-type` attribute set on binding root element when using runtime data binding

### When naming issues are found in .uss / .uxml

For any class name or `name` attribute that violates BEM or kebab-case rules, provide **both**:

1. The current (incorrect) value
2. The corrected replacement — e.g. `navBarMenu` → `navbar-menu`, `login-form-input` → `login-form__input`

List all affected selectors in the `.uss` file and all affected `class` / `name` attributes in the `.uxml` file so the developer can apply them as a batch find-and-replace.

---

## Output Format

When reviewing a **single file**:
```
## Unity Code Review — [FileName]

### Issues Found

| # | Severity | Category | Location | Issue | Fix |
|---|----------|----------|----------|-------|-----|
| 1 | Critical  | Naming   | PlayerController.Update | Field `health` missing `m_` prefix | Rename to `m_health` |
| 2 | Warning   | Allocation | EnemyAI.Update | `GetComponent<Renderer>()` called each frame | Cache in `Awake`: `m_renderer = GetComponent<Renderer>()` |
| 3 | Warning   | BEM Naming | MainMenu.uxml | `name="navbarMenu"` uses camelCase | Change to `name="navbar-menu"` |
| 4 | Critical  | USS Property | MainMenu.uss | `color: #FF0000` — hex not supported | Change to `color: rgb(255, 0, 0)` |

### Summary

- Critical: X
- Warning: Y
- Suggestion: Z

**Verdict: [Pass / Pass with warnings / Fail]**
```

When reviewing a **folder**, produce one section per file then a combined summary:
```
## Unity Code Review — [FolderPath/]

### PlayerController.cs
[issues table]

### MainMenu.uxml
[issues table]

### MainMenu.uss
[issues table]

---
### Combined Summary

- Files reviewed: N (.cs: X, .uxml: Y, .uss: Z)
- Critical: X  |  Warning: Y  |  Suggestion: Z

**Overall Verdict: [Pass / Pass with warnings / Fail]**
```

---

## Usage

**Claude Code** — invoke as a project skill:
```
/UnityCodeReview
```
Then specify what to review:
- Single file: `Review @Assets/Scripts/PlayerController.cs`
- Folder (recursive): `Review @Assets/Scripts/UI/`

**GitHub Copilot Chat** — reference this file and ask:
```
Using @UnityCodeReview.md, review the open file for style and performance issues.
```
Or for a folder:
```
Using @UnityCodeReview.md, review all files under Assets/Scripts/UI/ including subfolders.
```

**Any chat interface** — paste the checklist sections above along with your code/markup and ask the model to work through them systematically.
