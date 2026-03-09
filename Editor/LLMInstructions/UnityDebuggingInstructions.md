---
description: Guidelines for debugging Unity projects via MCP connection
applyTo: "**/*.cs"
---

# Unity Debugging Instructions for LLM Coding Tools

## Overview

This document provides instructions for AI coding assistants connected to Unity projects via MCP (Model Context Protocol).
Use these guidelines when helping developers debug Unity applications.

This file covers **cross-cutting debugging patterns** that apply to any Unity system. Domain-specific troubleshooting sections live inside each domain's instruction file:

| Domain | Troubleshooting in |
|--------|-------------------|
| Input System | `UnityInputSystemInstructions.md` |
| Animation | `UnityAnimationInstructions.md` |
| Audio | `UnityAudioInstructions.md` |
| UI Toolkit | `UnityUIToolkitInstructions.md` |
| Performance | `UnityPerformanceOptimizationInstructions.md` |

## Table of Contents

- [Diagnostic Priority Order](#diagnostic-priority-order)
- [Console Output Analysis](#console-output-analysis)
- [SerializeField and Inspector Debugging](#serializefield-and-inspector-debugging)
- [Script Execution Order Issues](#script-execution-order-issues)
- [Null Reference Debugging](#null-reference-debugging)
- [Physics Debugging](#physics-debugging)
- [Async and Coroutine Debugging](#async-and-coroutine-debugging)
- [Event System Debugging](#event-system-debugging)
- [ScriptableObject Runtime Issues](#scriptableobject-runtime-issues)
- [Transform and Hierarchy Issues](#transform-and-hierarchy-issues)
- [Scene and Asset Debugging](#scene-and-asset-debugging)
- [Build and Platform-Specific Debugging](#build-and-platform-specific-debugging)
- [Collaborative Debugging with AI Tools](#collaborative-debugging-with-ai-tools)
- [Debugging Checklist](#debugging-checklist)
- [LLM-Focused Triage Prompts](#llm-focused-triage-prompts)

## LLM-Focused Triage Prompts
- Paste the exact **error/warning text + stack trace** (include file path and line number).
- State **Unity version** (e.g., 6.3.x), **platform/build type** (Editor/Dev/Release), and **Enter Play Mode Options** (domain/scene reload on/off).
- Give **repro steps** and name the **scene/prefab** and **scripts** involved.
- For **Input/UI**, list the active **action map/control scheme** and **UXML/USS asset names** you are querying.
- Mention any **recent code or asset changes** just before the bug appeared.
- If possible, share a **minimal log snippet** around the failure (no screenshots).

## Diagnostic Priority Order

When investigating Unity issues, check these areas in order:

1. **Console errors and warnings** - Always start here
2. **Null reference exceptions** - Most common Unity issue
3. **Serialization state** - Inspector values vs runtime values
4. **Lifecycle timing** - Script execution order problems
5. **Scene/Prefab state** - Missing references, disabled objects
6. **Physics/Rendering settings** - Layer masks, culling, collision matrices

---

## Console Output Analysis

### Error Categories

| Prefix | Meaning | Typical Cause |
|--------|---------|---------------|
| `NullReferenceException` | Missing object reference | Unassigned SerializeField, destroyed object, wrong execution order |
| `MissingReferenceException` | Reference to destroyed object | Accessing object after Destroy() |
| `MissingComponentException` | GetComponent returned null | Component not attached, wrong type |
| `IndexOutOfRangeException` | Array/List bounds exceeded | Off-by-one errors, empty collections |
| `InvalidOperationException` | Operation not allowed | Modifying collection while iterating |

### Stack Trace Interpretation

```
NullReferenceException: Object reference not set to an instance of an object
  at PlayerController.Update () [0x00012] in Assets/Scripts/PlayerController.cs:47
  at UnityEngine.Internal.$MethodUtility.InvokeMethod (...)
```

**Key information:**
- Script name: `PlayerController`
- Method: `Update()`
- Line number: `47`
- File path: `Assets/Scripts/PlayerController.cs`

### Common Warning Patterns

```csharp
// Warning: "SendMessage cannot be called during Awake, CheckConsistency, or OnValidate"
// Fix: Defer to Start() or use Invoke/Coroutine

// Warning: "The referenced script on this Behaviour is missing!"
// Fix: Script file deleted or class name doesn't match filename

// Warning: "You are trying to create a MonoBehaviour using the 'new' keyword"
// Fix: Use AddComponent<T>() or Instantiate()
```

---

## SerializeField and Inspector Debugging

### Verify Serialization State

```csharp
// Check if field is properly serialized
[SerializeField] private GameObject _target;  // Visible in Inspector

// Common issues:
private GameObject _target;           // NOT serialized - won't appear in Inspector
public GameObject target;             // Serialized but exposed publicly (avoid)
[HideInInspector] public GameObject target;  // Serialized but hidden (intentional)
```

### Runtime vs Inspector Values

```csharp
// Debug serialized values at runtime
private void OnValidate()
{
    Debug.Log($"[Editor] _target assigned: {_target != null}");
}

private void Awake()
{
    Debug.Log($"[Runtime] _target assigned: {_target != null}");
}
```

### Check for Prefab Overrides

When a SerializeField appears assigned in the Prefab but null at runtime:
1. Check if the scene instance has an override (bold in Inspector)
2. Check if the value was cleared in a prefab variant
3. Check if OnValidate() or Reset() is clearing the value

---

## Script Execution Order Issues

### Unity Lifecycle Order

```
[Script Execution Order]
    ↓
Awake()           ← Object initialization, references to self
    ↓
OnEnable()        ← Subscribe to events
    ↓
Start()           ← References to other objects, initialization that depends on others
    ↓
FixedUpdate()     ← Physics updates (fixed timestep)
    ↓
Update()          ← Game logic (every frame)
    ↓
LateUpdate()      ← Camera follow, post-processing
    ↓
OnDisable()       ← Unsubscribe from events
    ↓
OnDestroy()       ← Cleanup
```

### Diagnosing Order Problems

```csharp
// Add execution order logging
private void Awake()
{
    Debug.Log($"{GetType().Name}.Awake() on {gameObject.name}", this);
}

private void Start()
{
    Debug.Log($"{GetType().Name}.Start() on {gameObject.name}", this);
}
```

### Setting Script Execution Order

```csharp
// Force early execution
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour { }

// Force late execution
[DefaultExecutionOrder(100)]
public class UIManager : MonoBehaviour { }
```

### Common Timing Issues

| Symptom | Likely Cause | Solution |
|---------|--------------|----------|
| Reference null in `Awake()` | Other object not yet initialized | Move to `Start()` |
| Reference null in `Start()` | Object created later in scene | Use `FindObjectOfType` with null check or events |
| Camera jitter | Camera in `Update()`, target in `Update()` | Move camera to `LateUpdate()` |
| Physics inconsistency | Physics logic in `Update()` | Move to `FixedUpdate()` |

- If state resets or Awake/OnEnable timing looks odd, verify **Enter Play Mode Options** (Project Settings → Editor). Domain reload off keeps static state; scene reload off keeps scene state.

---

## Null Reference Debugging

### Systematic Null Checking

```csharp
// Pattern: Validate all SerializeFields in Awake/Start
private void Awake()
{
    Debug.Assert(_playerTransform != null, "PlayerTransform not assigned!", this);
    Debug.Assert(_healthBar != null, "HealthBar not assigned!", this);
    Debug.Assert(_audioSource != null, "AudioSource not assigned!", this);
}

// Pattern: Null-conditional for optional references
_optionalComponent?.DoSomething();

// Pattern: Explicit null check with error
if (_requiredComponent == null)
{
    Debug.LogError($"Required component missing on {gameObject.name}", this);
    enabled = false;
    return;
}
```

### GetComponent Failure Patterns

```csharp
// Issue: Component on different GameObject
Rigidbody rb = GetComponent<Rigidbody>();  // Only checks THIS object

// Fix: Specify where to look
Rigidbody rb = GetComponentInChildren<Rigidbody>();
Rigidbody rb = GetComponentInParent<Rigidbody>();
Rigidbody rb = someOtherObject.GetComponent<Rigidbody>();

// Issue: Component added at runtime not yet available
// Fix: Use RequireComponent or check timing
[RequireComponent(typeof(Rigidbody))]
public class PhysicsController : MonoBehaviour { }
```

### Destroyed Object Access

```csharp
// Issue: Accessing destroyed object
private GameObject _enemy;

private void Update()
{
    // This throws MissingReferenceException after enemy is destroyed
    float dist = Vector3.Distance(transform.position, _enemy.transform.position);
}

// Fix: Unity's fake null check
if (_enemy != null)  // Works for destroyed objects
{
    float dist = Vector3.Distance(transform.position, _enemy.transform.position);
}

// Note: C# null check doesn't catch destroyed objects
if (_enemy is not null)  // WRONG - doesn't detect destroyed Unity objects
```

---

## Physics Debugging

### Layer and Collision Issues

```csharp
// Debug: Check layer configuration
Debug.Log($"Object layer: {gameObject.layer} ({LayerMask.LayerToName(gameObject.layer)})");

// Debug: Visualize raycast
Debug.DrawRay(origin, direction * maxDistance, Color.red, 2f);

// Debug: Check what layers a LayerMask includes
private void LogLayerMask(LayerMask mask)
{
    for (int i = 0; i < 32; i++)
    {
        if ((mask.value & (1 << i)) != 0)
        {
            Debug.Log($"Layer {i}: {LayerMask.LayerToName(i)}");
        }
    }
}
```

### Collision Matrix Verification

Check Edit → Project Settings → Physics (or Physics 2D):
- Verify layer collision matrix has expected checkboxes enabled
- Both objects must be on layers that collide with each other

### Rigidbody Issues

| Symptom | Check | Solution |
|---------|-------|----------|
| No collision detected | Is Rigidbody present? | Add Rigidbody to at least one object |
| Collision but no callback | Is trigger enabled incorrectly? | Match OnCollision vs OnTrigger methods |
| Objects pass through | Is one kinematic with no continuous detection? | Enable Continuous collision detection |
| Jittery movement | Moving in Update() | Move physics objects in FixedUpdate() |

### Trigger vs Collision Methods

```csharp
// Colliders (IsTrigger = false)
private void OnCollisionEnter(Collision collision) { }
private void OnCollisionStay(Collision collision) { }
private void OnCollisionExit(Collision collision) { }

// Triggers (IsTrigger = true)
private void OnTriggerEnter(Collider other) { }
private void OnTriggerStay(Collider other) { }
private void OnTriggerExit(Collider other) { }

// 2D variants
private void OnCollisionEnter2D(Collision2D collision) { }
private void OnTriggerEnter2D(Collider2D other) { }
```

---

## Async and Coroutine Debugging

### Coroutine Issues

```csharp
// Issue: Coroutine stops unexpectedly
// Cause: GameObject disabled or destroyed

// Debug: Track coroutine lifecycle
private IEnumerator MyCoroutine()
{
    Debug.Log("Coroutine started");
    try
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log("Coroutine tick");
        }
    }
    finally
    {
        Debug.Log("Coroutine ended");  // Called even when stopped
    }
}

// Issue: Multiple coroutines running
// Fix: Store and stop reference
private Coroutine _currentCoroutine;

public void StartMyCoroutine()
{
    if (_currentCoroutine != null)
        StopCoroutine(_currentCoroutine);
    _currentCoroutine = StartCoroutine(MyCoroutine());
}
```

### Async/Await Issues (Legacy Pattern)

```csharp
// Issue: Async method continues after object destroyed
private async void Start()
{
    await SomeAsyncOperation();
    // This line may execute after Destroy()!
    transform.position = Vector3.zero;  // Potential error
}

// Fix: Check for destruction
private async void Start()
{
    await SomeAsyncOperation();

    if (this == null) return;  // Unity's destroyed check

    transform.position = Vector3.zero;
}
```

### Awaitable API (Unity 6)

```csharp
// Unity 6 preferred pattern: Use Awaitable with destroyCancellationToken
private async Awaitable DoSomethingAsync()
{
    // Automatically cancelled when MonoBehaviour is destroyed
    await Awaitable.WaitForSecondsAsync(1f, destroyCancellationToken);

    // Safe to access - this line won't run if destroyed
    transform.position = Vector3.zero;
}

// Wait for next frame
private async Awaitable WaitOneFrame()
{
    await Awaitable.NextFrameAsync(destroyCancellationToken);
}

// Wait for end of frame
private async Awaitable WaitEndOfFrame()
{
    await Awaitable.EndOfFrameAsync(destroyCancellationToken);
}

// Wait for fixed update
private async Awaitable WaitForPhysics()
{
    await Awaitable.FixedUpdateAsync(destroyCancellationToken);
}

// Custom cancellation
private CancellationTokenSource _cts;

private async Awaitable DoWithCustomCancellation()
{
    _cts = new CancellationTokenSource();

    try
    {
        await Awaitable.WaitForSecondsAsync(5f, _cts.Token);
        Debug.Log("Completed");
    }
    catch (OperationCanceledException)
    {
        Debug.Log("Cancelled");
    }
}

public void Cancel() => _cts?.Cancel();
```

### Awaitable vs Coroutine Comparison

| Feature | Coroutine | Awaitable (Unity 6) |
|---------|-----------|---------------------|
| Cancellation | Manual StopCoroutine | Built-in token support |
| Return values | Not supported | Supported via `Awaitable<T>` |
| Exception handling | Limited | Full try/catch support |
| Destruction safety | Must check manually | `destroyCancellationToken` |
| Syntax | `yield return` | `async/await` |

---

## Event System Debugging

### UnityEvent Issues

```csharp
// Issue: UnityEvent not firing
// Debug: Check listener count
Debug.Log($"Listener count: {_onPlayerDeath.GetPersistentEventCount()}");

// Check: Are listeners assigned in Inspector?
// Check: Is the target object not destroyed?
// Check: Is the method signature correct?

// Debug: Log when event fires
[SerializeField] private UnityEvent _onPlayerDeath;

public void Die()
{
    Debug.Log($"Invoking OnPlayerDeath with {_onPlayerDeath.GetPersistentEventCount()} listeners");
    _onPlayerDeath?.Invoke();
}
```

### C# Event Subscription Leaks

```csharp
// Issue: Event keeps firing after object should be done
// Cause: Forgot to unsubscribe

// BAD: Memory leak and potential errors
private void Start()
{
    GameManager.OnGameOver += HandleGameOver;  // Subscribes
    // Never unsubscribes!
}

// GOOD: Always unsubscribe
private void OnEnable()
{
    GameManager.OnGameOver += HandleGameOver;
}

private void OnDisable()
{
    GameManager.OnGameOver -= HandleGameOver;
}

// Debug: Track subscriptions
public static class GameManager
{
    private static event Action _onGameOver;

    public static event Action OnGameOver
    {
        add
        {
            Debug.Log($"Subscriber added: {value.Target?.GetType().Name}.{value.Method.Name}");
            _onGameOver += value;
        }
        remove
        {
            Debug.Log($"Subscriber removed: {value.Target?.GetType().Name}.{value.Method.Name}");
            _onGameOver -= value;
        }
    }
}
```

### Event Invocation Order

```csharp
// Issue: Events firing in wrong order
// Debug: Log invocation order

public event Action<int> OnScoreChanged;

private void AddScore(int points)
{
    _score += points;

    // Log each subscriber as it's called
    if (OnScoreChanged != null)
    {
        int index = 0;
        foreach (var handler in OnScoreChanged.GetInvocationList())
        {
            Debug.Log($"Calling handler {index++}: {handler.Target?.GetType().Name}.{handler.Method.Name}");
            ((Action<int>)handler).Invoke(_score);
        }
    }
}
```

### Common Event Issues

| Symptom | Check | Solution |
|---------|-------|----------|
| Event never fires | Is Invoke() called? | Add null check and Invoke |
| Event fires multiple times | Subscribed multiple times? | Unsubscribe in OnDisable |
| NullReference on Invoke | No subscribers? | Use `?.Invoke()` pattern |
| Wrong object receives event | Static event with instance handler? | Unsubscribe on disable/destroy |

---

## ScriptableObject Runtime Issues

### Instance vs Asset Confusion

```csharp
// Issue: Runtime changes persist in Editor
[SerializeField] private PlayerDataSO _playerData;

private void TakeDamage(int damage)
{
    _playerData.health -= damage;  // Modifies the ASSET in Editor!
}

// Fix: Create runtime instance
private PlayerDataSO _runtimeData;

private void Awake()
{
    // Create a copy for runtime modifications
    _runtimeData = Instantiate(_playerData);
}

private void TakeDamage(int damage)
{
    _runtimeData.health -= damage;  // Safe - modifies instance only
}

private void OnDestroy()
{
    // Clean up runtime instance
    if (_runtimeData != null)
        Destroy(_runtimeData);
}
```

### ScriptableObject as Runtime Data Container

```csharp
// Pattern: Create SO at runtime for data binding
public class RuntimeDataProvider : MonoBehaviour
{
    [SerializeField] private PlayerDataSO _template;

    private PlayerDataSO _runtimeData;

    public PlayerDataSO RuntimeData
    {
        get
        {
            if (_runtimeData == null)
            {
                _runtimeData = ScriptableObject.CreateInstance<PlayerDataSO>();
                _runtimeData.hideFlags = HideFlags.HideAndDontSave;

                // Copy initial values from template
                _runtimeData.health = _template.health;
                _runtimeData.maxHealth = _template.maxHealth;
            }
            return _runtimeData;
        }
    }
}
```

### Shared Reference Issues

```csharp
// Issue: Multiple objects sharing same SO modify each other's data
// This is INTENTIONAL for shared state, but a bug if unexpected

// Debug: Log which object is modifying
public void ModifyHealth(int delta, string source)
{
    Debug.Log($"Health modified by {delta} from {source}. New value: {health + delta}");
    health += delta;
}

// Fix if unintentional: Each object needs its own instance
private void Awake()
{
    _playerData = Instantiate(_playerData);
}
```

### Common ScriptableObject Issues

| Symptom | Check | Solution |
|---------|-------|----------|
| Changes persist after play mode | Modifying asset directly? | Use Instantiate() for runtime copy |
| Multiple objects share state | Same SO assigned to all? | Intentional: shared state. Unintentional: instantiate |
| SO reference null at runtime | Asset not in build? | Check Resources folder or Addressables |
| OnEnable called in Editor | Normal behavior | Use `Application.isPlaying` check |

---

## Transform and Hierarchy Issues

### Local vs World Space

```csharp
// Issue: Position not where expected
// Debug: Log both spaces
Debug.Log($"Local Position: {transform.localPosition}");
Debug.Log($"World Position: {transform.position}");
Debug.Log($"Parent: {transform.parent?.name ?? "None"}");

// Common confusion:
transform.position = new Vector3(0, 0, 0);      // World space
transform.localPosition = new Vector3(0, 0, 0); // Relative to parent

// Issue: Rotation behaving unexpectedly
Debug.Log($"Local Rotation: {transform.localEulerAngles}");
Debug.Log($"World Rotation: {transform.eulerAngles}");
```

### Parent-Child Relationship Bugs

```csharp
// Issue: Child transform not updating
// Check: Is parent transform being modified?

// Debug: Log hierarchy
private void LogHierarchy(Transform t, int depth = 0)
{
    string indent = new string(' ', depth * 2);
    Debug.Log($"{indent}{t.name} (pos: {t.localPosition}, scale: {t.localScale})");

    foreach (Transform child in t)
    {
        LogHierarchy(child, depth + 1);
    }
}

// Issue: SetParent not working as expected
transform.SetParent(newParent);              // Maintains world position
transform.SetParent(newParent, false);       // Maintains local position
transform.SetParent(newParent, true);        // Maintains world position (explicit)
```

### Scale Inheritance Issues

```csharp
// Issue: Child scaled unexpectedly
// Debug: Check lossy scale (world scale)
Debug.Log($"Local Scale: {transform.localScale}");
Debug.Log($"Lossy Scale: {transform.lossyScale}");  // Accumulated scale

// Issue: Non-uniform parent scale causes skewing
// Fix: Avoid non-uniform scale on parents, or unparent before scaling

// Get world scale without parent influence
Vector3 GetWorldScaleIndependent()
{
    Vector3 scale = transform.localScale;
    Transform parent = transform.parent;

    while (parent != null)
    {
        scale = Vector3.Scale(scale, parent.localScale);
        parent = parent.parent;
    }

    return scale;
}
```

### Common Transform Issues

| Symptom | Check | Solution |
|---------|-------|----------|
| Object in wrong position | Local vs world space? | Use correct position property |
| Object doesn't move with parent | Is it actually a child? | Verify hierarchy in Inspector |
| Scale looks wrong | Parent has non-uniform scale? | Avoid or compensate for parent scale |
| Rotation gimbal lock | Using Euler angles? | Use Quaternion for complex rotations |

---

## Scene and Asset Debugging

### Missing Reference Detection

```csharp
#if UNITY_EDITOR
[ContextMenu("Find Missing References")]
private void FindMissingReferences()
{
    var fields = GetType().GetFields(
        System.Reflection.BindingFlags.Public |
        System.Reflection.BindingFlags.NonPublic |
        System.Reflection.BindingFlags.Instance);

    foreach (var field in fields)
    {
        if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
        {
            var value = field.GetValue(this) as UnityEngine.Object;
            if (value == null)
            {
                Debug.LogWarning($"Missing reference: {field.Name}", this);
            }
        }
    }
}
#endif
```

### Prefab Issues

- **Prefab Mode**: Changes in Prefab Mode don't affect scene instances with overrides
- **Nested Prefabs**: Inner prefab changes may not propagate if outer prefab has overrides
- **Prefab Variants**: Check the base prefab for missing references

---

## Build and Platform-Specific Debugging

### Conditional Compilation

```csharp
#if UNITY_EDITOR
    Debug.Log("Editor only");
#endif

#if DEVELOPMENT_BUILD
    Debug.Log("Development build");
#endif

#if UNITY_STANDALONE_WIN
    // Windows-specific code
#elif UNITY_STANDALONE_OSX
    // macOS-specific code
#elif UNITY_IOS
    // iOS-specific code
#elif UNITY_ANDROID
    // Android-specific code
#endif
```

### Build-Only Issues

Common causes for "works in Editor, fails in build":
1. **Script stripping**: Add `[Preserve]` attribute or link.xml
2. **Assembly definitions**: Missing references in .asmdef
3. **Resources path**: Case sensitivity on some platforms
4. **Editor-only code**: Wrapped in `#if UNITY_EDITOR`

---

## Collaborative Debugging with AI Tools

AI coding assistants (like Claude Code) cannot directly control IDE debuggers but can effectively assist with debugging through a collaborative workflow.

### What AI Tools Can Do

| Capability | Method |
|------------|--------|
| Read IDE diagnostics | Access compiler errors and warnings via MCP |
| Analyze code flow | Trace logic paths, identify potential issues |
| Add diagnostic code | Insert targeted logging, assertions |
| Interpret errors | Explain stack traces, suggest causes |
| Suggest fixes | Propose code changes based on analysis |

### What Requires Human Interaction

| Capability | Reason |
|------------|--------|
| Set breakpoints | Requires IDE UI interaction |
| Step through code | Real-time interactive process |
| Inspect runtime variables | Requires active debug session |
| Evaluate watch expressions | Requires debugger context |
| View call stack at breakpoint | Requires paused execution |

### Effective Collaboration Workflow

**Step 1: Describe the Issue**
```
Error: NullReferenceException in PlayerController.Update() line 47
Repro: Start game, press jump button
Expected: Player jumps
Actual: Exception thrown, player frozen
```

**Step 2: AI Analyzes Code**
The AI reads relevant files, traces execution flow, and identifies suspects.

**Step 3: AI Adds Diagnostic Code**
```csharp
private void Update()
{
    // Diagnostic logging added by AI
    Debug.Log($"[DEBUG] _inputHandler: {_inputHandler != null}", this);
    Debug.Log($"[DEBUG] _characterController: {_characterController != null}", this);
    Debug.Log($"[DEBUG] _isGrounded: {_isGrounded}", this);

    HandleMovement();
    HandleJump();  // Line 47 - exception occurs here
}
```

**Step 4: Human Runs and Reports**
Share the console output with the AI.

**Step 5: AI Interprets and Fixes**
Based on logs, the AI identifies the null reference and proposes a fix.

### Programmatic Breakpoints

When you need the debugger to pause at a specific condition:

```csharp
private void Update()
{
    // Pause debugger when unexpected state occurs
    if (_health < 0)
    {
        Debug.LogError("Health went negative - breaking to debugger");
        System.Diagnostics.Debugger.Break();  // Rider/VS will pause here
    }

    // Conditional break with context logging
    if (_player == null && _wasPlayerValid)
    {
        Debug.LogError($"Player reference lost! Last valid frame: {_lastValidFrame}");
        System.Diagnostics.Debugger.Break();
    }
}
```

### Diagnostic Code Patterns

**Trace Method Entry/Exit**
```csharp
private void ProcessInput()
{
    Debug.Log($"→ {nameof(ProcessInput)} entered", this);

    // ... method logic ...

    Debug.Log($"← {nameof(ProcessInput)} exiting", this);
}
```

**State Snapshot**
```csharp
[ContextMenu("Log State Snapshot")]
private void LogStateSnapshot()
{
    Debug.Log("=== STATE SNAPSHOT ===");
    Debug.Log($"Position: {transform.position}");
    Debug.Log($"Velocity: {_rigidbody?.linearVelocity}");
    Debug.Log($"IsGrounded: {_isGrounded}");
    Debug.Log($"CurrentState: {_currentState}");
    Debug.Log($"InputVector: {_inputVector}");
    Debug.Log("======================");
}
```

**Conditional Logging (Editor Only)**
```csharp
[System.Diagnostics.Conditional("UNITY_EDITOR")]
private void DebugLog(string message)
{
    Debug.Log($"[{GetType().Name}] {message}", this);
}

// Usage - automatically stripped from builds
DebugLog($"Processing {_items.Count} items");
```

### Tips for Effective AI-Assisted Debugging

1. **Provide context**: Include error messages, stack traces, and repro steps
2. **Share console output**: Copy/paste the actual log output after running
3. **Describe expected vs actual**: What should happen vs what does happen
4. **Mention recent changes**: What was modified before the bug appeared
5. **Include relevant settings**: Unity version, platform, Enter Play Mode Options
