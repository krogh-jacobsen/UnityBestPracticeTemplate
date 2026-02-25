# Comprehensive Copilot Instructions for Strategy Game Template

**Consolidated Reference Guide** — Merges all project documentation into one authoritative source for Claude Code.

**Target Environment:**
- **Unity Version**: 6.3.4f1 exclusively
- **C# Version**: 9.0+ features supported
- **Render Pipeline**: URP 17.3.0
- **UI System**: UI Toolkit (not UGUI)
- **Input System**: Input System (not Input Manager)

---

## Table of Contents

### Fundamentals
1. [Unity Version-Specific Instructions](#unity-version-specific-instructions)
2. [Project Architecture Overview](#project-architecture-overview)

### Code Standards
3. [C# Code Style & Naming Conventions](#c-code-style--naming-conventions)
4. [Class Organization](#class-organization)
5. [Methods & Naming Patterns](#methods--naming-patterns)
6. [Events & Subscriptions](#events--subscriptions)

### Design & Architecture
7. [Design Patterns & SOLID](#design-patterns--solid)
8. [Project-Specific Patterns](#project-specific-patterns)

### UI Development
9. [UI Toolkit Complete Reference](#ui-toolkit-complete-reference)

### Debugging & Performance
10. [Debugging Instructions](#debugging-instructions)
11. [Performance Optimization](#performance-optimization)

### Project Configuration
12. [Project Configuration & Editor Settings](#project-configuration--editor-settings)

### Quick Reference
13. [Common Mistakes & Troubleshooting](#common-mistakes--troubleshooting)

---

## Unity Version-Specific Instructions

- ℹ️ This project uses **Unity 6.3**. Always use the latest documentation and APIs that apply to Unity 6 or later versions.
- ℹ️ Use the newer **Input System**, not the Input Manager.
- ℹ️ Use the newer **UI Toolkit**, not UGUI.
- ℹ️ Use **Universal Render Pipeline (URP)**, not the built-in render pipeline.
- ℹ️ Prefer **Unity 6 Awaitables** over coroutines: `await Awaitable.WaitForSecondsAsync(delay, token);` Guard continuations with `if (this == null || !isActiveAndEnabled) return;`.
- ℹ️ When instantiating frequently, favor `UnityEngine.Pool.ObjectPool<T>` with `actionOnGet`/`actionOnRelease` to toggle active state.
- ℹ️ Unity 6 introduces `[CreateProperty]` and data binding at runtime — use this for reactive UI updates instead of manual synchronization.
- ℹ️ Burst compiler is enabled. Math-heavy code can use Jobs + Burst for significant performance gains.
- ℹ️ IL2CPP builds have different performance characteristics than Mono — profile on target platform.

---

## Project Architecture Overview

### Centralized Event System

All game events flow through **`Assets/Scripts/Core/StaticGameEvents.cs`**. This is the single source of truth for inter-system communication.

**Key Events:**
- `OnTileSelected` / `OnArmySelected` — User selection
- `OnTurnStarted` / `OnTurnEnded` — Turn lifecycle
- `OnUIStateChanged` — UI state machine transitions
- `OnMapOwnershipChanged` / `OnMapTileAttacked` — Map changes
- `OnResourcesChanged` — Resource inventory updates
- `OnWarDeclared` / `OnPeaceDeclared` / `OnAgreementFormed` — Diplomacy events

**Pattern:** Static methods control event invocation to prevent external code from firing events inappropriately.

### UI State Machine

`Assets/UI Toolkit/Scripts/UIGameController.cs` manages UI state with enum `UIGameState`:
- `DefaultMapView` — Main game view
- `ArmyView` — Army selected
- `ArmyRecruitmentView` — Recruitment panel
- `EventPopupView` — Random events
- `TownView` / `TownConstructionView` / `ConquestView` — Tile interactions
- `DiplomacyView` — Faction diplomacy

### UI Toolkit Architecture

All UI views inherit from `Assets/UI Toolkit/Scripts/UITKBaseClass.cs`:

```csharp
protected abstract void InitializeElements();  // Cache UI elements
protected abstract void RegisterCallbacks();   // Subscribe to events
protected abstract void UnregisterCallbacks(); // Cleanup
public abstract void ShowPanel(bool show);     // Toggle visibility
```

### ScriptableObject Data Pattern

Game data is defined via ScriptableObjects (suffix with `DataSO` or `SO`):
- `ArmyUnitSO` — Unit definitions (stats, costs, category)
- `ArmyCommanderSO` — Commander attributes
- `FactionDataSO` — Faction visuals and colors
- `MapTileConfigDataSO` — Tile defaults

---

## C# Code Style & Naming Conventions

### General Guidelines

- ⚠️ **Readability is key.** Try to keep lines short. Consider horizontal whitespace.
- ✅ Use **Allman style** (opening curly braces on a new line).
- ✅ Define a standard **max line width of less than 120–140 characters**.
- ✅ Break a long line into smaller statements rather than letting it overflow.
- ✅ Use a single space before flow control conditions, e.g., `while (x == y)`.
- ❌ Avoid spaces inside brackets, e.g., `x = dataArray[index]`.

```csharp
// Good spacing example using Allman style braces and spacing
public void ProcessItems(List<Item> items, int startIndex)
{
    for (int i = startIndex; i < items.Count; i++)
    {
        ProcessItem(items[i]);
    }

    // Note vertical spacing here for visual separation
    Debug.Log("Processing complete");
}
```

### Spacing Rules

- ✅ Use a single space after a comma between function arguments, e.g., `CollectItem(myObject, 0, 1);`.
- ❌ Don't add spaces just inside parentheses before the first or after the last argument.
- ❌ Don't use spaces between a function name and parenthesis.
- ✅ Use vertical spacing (extra blank line) for visual separation.
- ✅ Use one variable declaration per line in most cases.
- ✅ Use a single space before and after comparison operators.

### Use of Regions

- ℹ️ Use `#region` sparingly as it can hide code and reduce readability.
- ✅ Use `#region` to group Animation Event Handlers or Input Event Handlers, called from the animation system.

### Comments

- ✅ Add clarifying comments to most lines for documentation.
- ✅ Comment intent ("why") rather than restating code ("what").
- ✅ Use `[Tooltip]`, `[Header]`, `[Space]`, etc. for serialized fields that need Inspector context.

```csharp
// Good - explains why, not just what
// Skip processing if below threshold to avoid performance issues
if (itemCount < processingThreshold)
{
    return;
}

[Tooltip("Maximum distance the player can travel in one frame")]
[SerializeField] private float m_maxDeltaMovement = 10f;
```

---

## Class Organization

Organize classes in the **Unity Script Execution Order**:

1. **Using statements** — System namespaces, then Unity, then project-specific
2. **Namespace declaration**
3. **Fields** — Serialized first, then private, static with `s_` prefix, constants with `k_` prefix
4. **Properties** — PascalCase, no prefixes
5. **Events** — Event declarations with `On` prefix for raise methods
6. **MonoBehaviour Methods** — In execution order (Awake → OnEnable → Start → Update → LateUpdate → OnDisable)
7. **Public Methods**
8. **Private Methods**
9. **Other Classes** (nested)

### Using Statements & Namespaces

- ✅ Keep using statements at the top of your file.
- ✅ Order: System namespaces → Unity namespaces → project-specific
- ✅ Remove unused `using` statements to keep the file clean.
- ✅ Use namespaces to prevent class conflicts. Use PascalCase without special symbols.

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Core.UI;

namespace Core.MapGameplay { ... }
```

### Fields

- ✅ Use `m_` prefix for private variables
- ✅ Use `s_` prefix for static fields
- ✅ Use `k_` prefix for constants
- ✅ Use descriptive names that clearly indicate the field's purpose.
- ❌ Avoid abbreviations unless they are widely understood (e.g., `UI`, `ID`).
- ✅ Include units in the name if applicable (e.g., `m_speedInMetersPerSecond`).
- ✅ Prefix Boolean fields with verbs like `is`, `has`, or `can` (e.g., `m_isActive`, `m_hasPermission`).
- ❌ Avoid redundancy by not repeating the class name in field names.
- ✅ Expose fields in the Inspector with `[SerializeField]`.

```csharp
private int m_health;                  // Use m_ prefix
private static int s_sharedCount;      // Use s_ prefix
private const int k_maxCount = 100;    // Use k_ prefix

[SerializeField] private int m_health; // Serialized private field

private int m_elapsedTimeInSeconds;    // Include units
private bool m_isPlayerDead;           // Prefix booleans
```

### Properties

- ✅ Place properties after fields and before MonoBehaviour methods.
- ✅ Use PascalCase for properties and avoid prefixes/suffixes.
- ✅ Prefer verb-like names for boolean properties (Is/Has/Can).
- ❌ Do not serialize properties. Instead use [SerializeField] private T m_field with a public property.
- ✅ Use Properties for accessing or modifying the state of an object.
- ℹ️ Use methods for actions or operations.

```csharp
// Private backing field
private int m_maxHealth;

// Read-only property
public int MaxHealthReadOnly => m_maxHealth;

// Property with full implementation
public int MaxHealth
{
    get => m_maxHealth;
    set => m_maxHealth = value;
}

// Auto-implemented property
public string DescriptionName { get; set; } = "Fireball";
```

---

## Events & Subscriptions

### Event Declaration

- ✅ Use `event Action` or `event Action<T>` for declaring events.
- ✅ Use UnityEvent only when you need to expose callbacks to the Inspector.
- ✅ Follow the C# event naming convention: use past tense verbs (e.g., `DoorOpened`).
- ✅ Use the `On` prefix for methods that raise events (e.g., `OnDoorOpened`).
- ✅ Use the observer pattern to decouple systems and reduce dependencies.
- ✅ Use the null-conditional operator (`?.`) when raising events.

```csharp
// Event declarations
public event Action DoorOpened;         // Use past tense verbs
public event Action<int> PointsScored;
public event Action<CustomEventArgs> ThingHappened;

// Event raising methods
public void OnDoorOpened()
{
    DoorOpened?.Invoke();
}
```

### Subscribing and Unsubscribing

- ✅ Subscribe in `OnEnable()` and always unsubscribe in `OnDisable()` to prevent memory leaks.
- ✅ Avoid using lambda expressions when subscribing to events — makes unsubscribing impossible.
- ⚠️ Be cautious when subscribing long-lived objects (e.g., singletons) to events from short-lived objects.

```csharp
private void OnEnable()
{
    m_gameManager.DoorOpened += HandleDoorOpened;
    StaticGameEvents.OnTileSelected += HandleTileSelected;
}

private void OnDisable()
{
    m_gameManager.DoorOpened -= HandleDoorOpened;
    StaticGameEvents.OnTileSelected -= HandleTileSelected;
}
```

---

## Methods & Naming Patterns

### Method Naming Conventions

- ✅ Use methods for behavior and event callbacks (actions, side effects, or inputs).
- ✅ Name methods with descriptive verbs to state the action clearly.
- ✅ Use clear prefixes: `SetX` for assigning/updating a value, `ChangeX` for modifying state.
- ✅ **Use "Process" for game logic operations** — turn-based, scheduled, or system-driven operations that are part of game flow. Examples: `ProcessTradeIncome()`, `ProcessModifierDecay()`.
- ✅ **Use "Handle" for event-driven callbacks** — responding to external input or events. Examples: `HandleTileSelected()`, `HandleTurnEnded()`.
- ✅ Boolean methods should pose a question and return bool, using Is, Has, or Can.
- ❌ Avoid noun-style method names except for factory methods or event handlers.

```csharp
// Good: Use a method for actions or operations

// Action: performs behavior / side effects
public void Jump()
{
    m_rigidbody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
}

// Setter: clearly assigns or updates a value
public void SetMovementInput(Vector2 input)
{
    m_forwardMovementInput = input;
}

// Modifier: transforms or changes state
public void ChangeHealth(int amount)
{
    m_health += amount;
}

// ✅ "Handle" for event-driven callbacks
private void HandleTileSelected(MapTile tile)
{
    ChangeState(UIGameState.TownView);
}

// ✅ "Process" for game logic operations
private void ProcessTradeIncome()
{
    foreach (var relationship in m_relationships)
    {
        if (relationship.InvolvesFaction(m_playerFactionData))
        {
            m_gameResources.ModifyCurrentGold(m_tradeAgreementGold);
        }
    }
}

// Boolean methods asking questions
public bool IsPlayerAlive() => m_health > 0;
```

---

## Design Patterns & SOLID

### SOLID Principles

**S** — Single Responsibility: A class should have only one reason to change. Break down large classes into smaller, focused classes.

**O** — Open/Closed: Classes must be open for extension but closed for modification.

**L** — Liskov Substitution: Subtypes must be substitutable for their base types without altering the correctness of the program.

**I** — Interface Segregation: No class should be forced to implement interfaces it doesn't use. Prefer small, focused interfaces.

**D** — Dependency Inversion: High-level modules should not depend on low-level modules; both should depend on abstractions.

### Interfaces

- ✅ Use interfaces to define clear "contracts" and decouple systems.
- ✅ Use the one responsibility rule per interface (Interface Segregation).
- ✅ Use the `I` prefix and PascalCase (e.g., `IDamageable`, `IAudioService`).
- ✅ Name methods with verbs and boolean members with Is/Has/Can.
- ✅ Use an interface for a pure contract with no shared implementation.

```csharp
public interface IDamageable
{
    string DamageTypeName { get; }
    float DamageValue { get; }

    bool ApplyDamage(string description, float damage, int numberOfHits);
}
```

### Enums for State Management

- ✅ Use enums for mutually exclusive states (e.g., animation, movement, UI, or game phases).
- ✅ Use enums in switch statements for clear, maintainable logic.
- ❌ Avoid using strings or integers directly for state tracking.
- ✅ Use PascalCase for enum names and values.

```csharp
public enum Direction
{
    North,
    South,
    East,
    West
}

private void Update()
{
    switch (m_currentDirection)
    {
        case Direction.North:
            // Move north
            break;
    }
}
```

### String Operations & Allocation Management

- ✅ Use string interpolation (`$""`) for building strings instead of concatenation (`+`).
- ✅ Use `StringBuilder` for building strings dynamically in loops.

```csharp
// Good - use string interpolation
private void UpdateScoreDisplay(int score, float time)
{
    string result = $"Score: {score} Time: {time:F1}";
}
```

### Collection Type Selection

- ✅ Use `List<T>` when the collection size changes dynamically.
- ✅ Use arrays when the size is fixed and performance matters.
- ✅ Use `Stack<T>` for Last-In-First-Out (LIFO) logic.
- ✅ Use `Dictionary<TKey, TValue>` for fast lookups by key.
- ❌ Avoid allocations inside loops — reuse collections and call `.Clear()`.

```csharp
private List<Enemy> m_enemies = new();
private Enemy[] m_enemyPool = new Enemy[100];
private Dictionary<int, Enemy> m_enemyById = new();
```

### Async & Awaitable Usage

- ✅ Use the **Awaitable API** (available in Unity 6+) with async/await for timed delays.
- ✅ Name async methods with the Async suffix (e.g., `OpenDoorAsync`).
- ✅ Prefer Awaitable over StartCoroutine for simple delays.

```csharp
public async Awaitable OpenDoorAsync()
{
    Debug.Log("Door opening...");
    await Awaitable.WaitForSecondsAsync(2f, destroyCancellationToken);
    Debug.Log("Door opened!");
}
```

### ScriptableObjects

- ✅ Favor ScriptableObjects for static configuration data.
- ❌ Don't use ScriptableObjects to store data that changes during gameplay.
- ✅ Always mark ScriptableObjects with `[CreateAssetMenu]`.
- ✅ Append a `DataSO` suffix (e.g., `WeaponDataSO`).

```csharp
[CreateAssetMenu(fileName = "WeaponData", menuName = "Game Data/Weapon")]
public class WeaponDataSO : ScriptableObject
{
    [SerializeField] private string m_weaponName;
    [SerializeField] private int m_damage;

    public string WeaponName => m_weaponName;
    public int Damage => m_damage;
}
```

### Animation Parameters, Tags, and Input Action Names

- ✅ **PascalCase** is recommended for all text-based references.
- ✅ Prefix boolean animation parameters with **Is**, **Has**, or **Can**.
- ✅ Always define these names as constants in code.

```csharp
private const string k_isRunningParam = "IsRunning";
private const string k_speedParam = "Speed";

m_animator.SetBool(k_isRunningParam, isMoving);
m_animator.SetFloat(k_speedParam, currentSpeed);
```

### Debugging

- ✅ Log strategically: use `Debug.Log`, `Debug.LogWarning`, and `Debug.LogError` selectively.
- ✅ Use conditional compilation (e.g., `#if UNITY_EDITOR`) to strip logs in release builds.
- ✅ Always include context in log messages.
- ✅ When using `Debug.Log`, pass a GameObject or component as the second parameter.

```csharp
// Include context in log messages
Debug.Log("Player has entered the trigger zone.", this.gameObject);

// Better error logging
Debug.LogError($"[{GetType().Name}] Failed to load data: {exception.Message}", this);
```

### Using Try-Catch & Debugger Breaks

- ✅ Use try-catch blocks for handling external dependencies (file I/O, network requests).
- ❌ Avoid using try-catch for internal logic or expected conditions.
- ✅ Always log the exception details.

```csharp
public void SaveGame(GameData data)
{
    try
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(k_saveFilePath, json);
    }
    catch (Exception ex)
    {
        Debug.LogError($"[{GetType().Name}] Error saving: {ex}", this);
        ShowSaveErrorToPlayer();
    }
}
```

---

## Project-Specific Patterns

### Observer Pattern (Centralized Static Events)

**Location:** `StaticGameEvents.cs`

All game systems communicate through a single static event bus. Events are public for subscribing, but invocation is controlled via static methods.

```csharp
public static class StaticGameEvents
{
    public static event Action<MapTile> OnTileSelected;
    public static event Action<UIGameState> OnUIStateChanged;

    public static void InvokeOnTileSelected(MapTile tile) => OnTileSelected?.Invoke(tile);
    public static void InvokeOnUIStateChanged(UIGameState newState) => OnUIStateChanged?.Invoke(newState);
}
```

### State Pattern (Enum + Switch)

**Location:** `UIGameController.cs`

UI state is managed with an enum and switch statement.

```csharp
public enum UIGameState
{
    DefaultMapView,
    ArmyView,
    TownView,
}

public void ChangeState(UIGameState newState)
{
    m_currentState = newState;
    ApplyStateToPanels(newState);
}
```

### Template Method Pattern

**Location:** `UITKBaseClass.cs`

All UI views inherit from a base class that enforces a consistent lifecycle.

```csharp
public abstract class UITKBaseClass : MonoBehaviour
{
    protected virtual void Awake()
    {
        m_uiDocument = GetComponent<UIDocument>();
        InitializeElements();
    }

    protected virtual void OnEnable()
    {
        RegisterCallbacks();
    }

    protected virtual void OnDisable()
    {
        UnregisterCallbacks();
    }

    protected abstract void InitializeElements();
    protected abstract void RegisterCallbacks();
    protected abstract void UnregisterCallbacks();
    public abstract void ShowPanel(bool show);
}
```

### Service Locator & Dependency Injection

**Location:** `ServiceLocator.cs` / `DependencyInjector.cs`

Services are registered at startup and resolved by dependent systems.

```csharp
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> s_services = new();

    public static void Register<T>(T service) where T : class
    {
        s_services[typeof(T)] = service;
    }

    public static T Resolve<T>() where T : class
    {
        if (s_services.TryGetValue(typeof(T), out object service))
            return service as T;
        throw new InvalidOperationException($"Service not registered: {typeof(T)}");
    }
}
```

### Composition over Inheritance

**Location:** MapTile system

Rather than deep inheritance hierarchies, use focused MonoBehaviour components on the same GameObject.

```csharp
public class MapTile : MonoBehaviour
{
    [SerializeField] private MapTileMilitary m_mapTileMilitary;

    private void Awake()
    {
        m_mapTileMilitary = GetComponent<MapTileMilitary>();
    }
}
```

### Object Pooling

- ✅ Use `UnityEngine.Pool.ObjectPool<T>` for frequently spawned/despawned objects.
- ✅ Pre-warm pools with a reasonable number of objects.
- ✅ Always reset pooled object state before reusing.

```csharp
public class BulletPool : MonoBehaviour
{
    [SerializeField] private Bullet m_bulletPrefab;
    private ObjectPool<Bullet> m_pool;

    private void Awake()
    {
        m_pool = new ObjectPool<Bullet>(
            createFunc: () => Instantiate(m_bulletPrefab),
            actionOnGet: bullet => bullet.gameObject.SetActive(true),
            actionOnRelease: bullet => bullet.gameObject.SetActive(false),
            defaultCapacity: 20,
            maxSize: 100
        );
    }

    public Bullet GetFromPool() => m_pool.Get();
    public void ReturnToPool(Bullet bullet) => m_pool.Release(bullet);
}
```

---

## UI Toolkit Complete Reference

### File Naming & Organization

- ✅ Use **PascalCase** for UXML and USS filenames (e.g., `MainMenu.uxml`, `PlayerHUD.uss`).
- ✅ Organize UXML and USS files in a consistent folder structure.
- ✅ Name USS files to match their corresponding UXML files.

### Critical USS vs CSS Differences

**USS (Unity Style Sheets) is NOT standard CSS.** It is a subset with Unity-specific extensions.

| Feature | CSS | USS |
|---------|-----|-----|
| Layout model | Flexbox + Grid | **Flexbox only** |
| Length units | `px`, `%`, `em`, `rem` | **`px` and `%` only** |
| `calc()` | ✅ Supported | ❌ Not supported |
| Color values | Hex `#FF6432` | **`rgb()` and `rgba()` only** |
| Text alignment | `text-align: center` | `-unity-text-align: middle-center` |
| `:nth-child()` | ✅ | ❌ Not supported |

**Color Values — Critical:**
```css
/* ✅ USS - use rgb() or rgba() only */
background-color: rgb(255, 100, 50);

/* ❌ Hex values do NOT work in USS */
background-color: #FF6432;  /* FAILS */
```

### USS Naming Conventions (BEM)

- ✅ Use **kebab-case** for UXML `name` and `class` values.
- ✅ Use **BEM** (Block-Element-Modifier) pattern: `block-name__element-name--modifier-name`
- ✅ Use `name` for unique identifiers (elements queried in C#).
- ✅ Use `class` for reusable styles.

**Examples:**
- ✅ `navbar-menu__item`, `button--primary`, `login-form__input-field--error`
- ❌ `navbar-item` (missing block), `button-primary` (missing `--`)

### Flexbox Layout System

**Container Properties (Parent):**
```css
flex-direction: column;         /* Default - vertical */
flex-direction: row;            /* Horizontal layout */
justify-content: flex-start;   /* Main axis alignment */
justify-content: center;
align-items: stretch;          /* Cross axis alignment */
align-items: center;
/* ⚠️ gap is NOT supported in USS — use margin on children instead */
```

**Item Properties (Children):**
```css
flex-grow: 1;                  /* Grow to fill available space */
flex-shrink: 0;                /* Don't shrink */
flex: 1;                       /* Shorthand */
```

### USS Common Properties Reference

**Display & Visibility:**
```css
display: flex;                 /* Visible, in layout */
display: none;                 /* Hidden, removed from layout */
visibility: visible;           /* Default */
visibility: hidden;            /* Hidden, keeps space */
opacity: 1;                    /* 0 to 1 */
```

**Sizing:**
```css
width: 100px;
width: 50%;
height: 100px;
min-width: 50px;
max-width: 200px;
flex-grow: 1;
```

**Spacing:**
```css
padding: 10px;
margin: 10px;
margin-left: auto;             /* Push to right */
```

**Borders & Backgrounds:**
```css
border-width: 2px;
border-color: rgb(0, 0, 0);
border-radius: 8px;
background-color: rgb(50, 50, 50);
background-image: url('project://database/Assets/UI/bg.png');
```

**Text:**
```css
color: rgb(0, 0, 0);
font-size: 16px;
-unity-font-style: bold;
-unity-text-align: middle-center;
```

**Pseudo-Classes (Only These Are Supported):**
```css
.button:hover { }
.button:active { }
.button:focus { }
.button:disabled { }
.toggle:checked { }
```

### USS Variables (Design Tokens)

Variables must be declared in `:root {}`:

```css
:root {
    --color-primary: rgb(72, 144, 226);
    --color-surface: rgb(40, 40, 40);
    --spacing-sm: 8px;
    --spacing-md: 16px;
}

.card {
    background-color: var(--color-surface);
    padding: var(--spacing-md);
}
```

### UXML Structure

**File Structure:**
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <Style src="project://database/Assets/UI/Styles/MainStyles.uss" />
    <ui:VisualElement name="root" class="container">
        <!-- Content -->
    </ui:VisualElement>
</ui:UXML>
```

### Data Binding (Unity 6+)

**Model with Reactive Updates:**

```csharp
[CreateAssetMenu]
public class PlayerDataSO : ScriptableObject, INotifyBindablePropertyChanged
{
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    [SerializeField] private int m_health = 100;

    [CreateProperty]
    public int Health
    {
        get => m_health;
        set
        {
            if (m_health != value)
            {
                m_health = value;
                propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(nameof(Health)));
            }
        }
    }
}
```

**Binding in UXML:**
```xml
<ui:VisualElement data-source-type="PlayerDataSO, Assembly-CSharp">
    <ui:ProgressBar binding-path="Health" low-value="0" high-value="100">
        <Bindings>
            <ui:DataBinding property="value" binding-mode="ToTarget" />
        </Bindings>
    </ui:ProgressBar>
</ui:VisualElement>
```

**Setting Data Source in C#:**
```csharp
private void OnEnable()
{
    var root = m_uiDocument.rootVisualElement;
    var panel = root.Q<VisualElement>("player-panel");
    panel.dataSource = m_playerData;
}
```

### Querying Elements

```csharp
// By name
var button = root.Q<Button>("submit-button");

// By type only
var firstLabel = root.Q<Label>();

// By USS class
var cards = root.Query<VisualElement>(className: "card").ToList();

// Cache queries in OnEnable (NOT Update)
private void OnEnable()
{
    var root = m_uiDocument.rootVisualElement;
    m_submitButton = root.Q<Button>("submit-button");
    m_submitButton.clicked += OnSubmitClicked;
}

private void OnDisable()
{
    m_submitButton.clicked -= OnSubmitClicked;
}
```

### Show/Hide Patterns

```csharp
// Hide — removes from layout
element.style.display = DisplayStyle.None;

// Show — returns to layout
element.style.display = DisplayStyle.Flex;

// Hide but keep space
element.style.visibility = Visibility.Hidden;

// Helper method
public void SetPanelVisible(bool isVisible)
{
    m_panel.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
}
```

### Button & Event Handling

```csharp
private void OnEnable()
{
    m_actionButton = root.Q<Button>("action-button");
    m_actionButton.clicked += OnActionButtonClicked;
}

private void OnDisable()
{
    m_actionButton.clicked -= OnActionButtonClicked;
}

private void OnActionButtonClicked()
{
    Debug.Log("Action button clicked");
}
```

### ListView & Template Spawning

```csharp
// Manual template instantiation
private void PopulateCards()
{
    foreach (var cardData in m_cards)
    {
        var cardElement = m_cardTemplate.Instantiate();
        cardElement.Q<Label>("card-title").text = cardData.Title;
        cardElement.dataSource = cardData;
        m_cardContainer.Add(cardElement);
    }
}

// ListView with virtualization
private void SetupListView()
{
    m_listView.makeItem = () => m_itemTemplate.Instantiate();
    m_listView.bindItem = (element, index) =>
    {
        var item = m_items[index];
        element.Q<Label>("item-name").text = item.ItemName;
    };
    m_listView.itemsSource = m_items;
}
```

### Performance Tips for UI Toolkit

1. **Cache VisualElement references** in `OnEnable` — never call `Q<>()` in `Update`
2. **Use USS classes** for style changes, not inline `element.style.*`
3. **Use `ListView`** for lists with more than ~20 items
4. **Avoid `Query<>().ToList()`** in per-frame code
5. **Use USS variables** for colours and sizes
6. **Minimize UXML nesting** — each level adds traversal cost

---

## Debugging Instructions

### Diagnostic Priority Order

When investigating Unity issues, check these areas in order:

1. Console errors and warnings
2. Null reference exceptions
3. Serialization state (Inspector values vs runtime)
4. Lifecycle timing (Script execution order)
5. Scene/Prefab state
6. Physics/Rendering settings

### Console Output Analysis

| Prefix | Meaning | Typical Cause |
|--------|---------|---------------|
| `NullReferenceException` | Missing object reference | Unassigned SerializeField, destroyed object |
| `MissingComponentException` | GetComponent returned null | Component not attached |
| `IndexOutOfRangeException` | Array/List bounds exceeded | Off-by-one errors |

### Script Execution Order Issues

```
Awake()           ← Object initialization
    ↓
OnEnable()        ← Subscribe to events
    ↓
Start()           ← Initialization depending on other objects
    ↓
FixedUpdate()     ← Physics updates
    ↓
Update()          ← Game logic
    ↓
LateUpdate()      ← Camera follow
    ↓
OnDisable()       ← Unsubscribe from events
```

**Diagnosing Order Problems:**
```csharp
private void Awake()
{
    Debug.Log($"{GetType().Name}.Awake() on {gameObject.name}", this);
}
```

### Null Reference Debugging

```csharp
// Pattern: Validate all SerializeFields in Awake
private void Awake()
{
    Debug.Assert(_playerTransform != null, "PlayerTransform not assigned!", this);
    if (_requiredComponent == null)
    {
        Debug.LogError($"Required component missing on {gameObject.name}", this);
        enabled = false;
        return;
    }
}
```

### UI Toolkit Debugging

```csharp
// Element not found
var button = root.Q<Button>("myButton");  // Returns null if not found

// Debug: List all elements
root.Query().ForEach(e => Debug.Log($"{e.GetType().Name}: {e.name}"));

// Data binding issues
// Check: binding-path is case-sensitive
// Check: Property has [CreateProperty] attribute
// Check: Class implements INotifyBindablePropertyChanged
// Check: dataSource is assigned in C#
```

### Event System Debugging

```csharp
// Check listener count
Debug.Log($"Listener count: {_onPlayerDeath.GetPersistentEventCount()}");

// Debug subscriptions
public static event Action OnGameOver
{
    add { Debug.Log($"Subscriber added: {value.Target?.GetType().Name}"); }
    remove { Debug.Log($"Subscriber removed: {value.Target?.GetType().Name}"); }
}
```

---

## Performance Optimization

### Code Review Priority Checklist

When reviewing Unity code for performance, check in this order:

1. **Update loops** — Allocations, expensive operations, unnecessary work
2. **Physics** — OverlapSphere, Raycast frequency
3. **Memory** — String concatenation, LINQ in hot paths, boxing
4. **GetComponent/Find** — Uncached lookups, per-frame calls
5. **Rendering** — Material instances, draw calls

### Update Loop Optimization

**Never allocate in Update(), FixedUpdate(), or LateUpdate()** — This triggers garbage collection spikes.

```csharp
// ❌ Bad - allocates every frame
private void Update()
{
    var enemies = FindObjectsOfType<Enemy>();
    var nearbyEnemies = new List<Enemy>();
    string status = $"Enemies: {enemies.Length}";
}

// ✅ Good - zero allocations
private Enemy[] m_enemyCache = new Enemy[100];
private readonly List<Enemy> m_nearbyEnemies = new(50);

private void Update()
{
    int count = FindObjectsOfType(m_enemyCache);
    m_nearbyEnemies.Clear();
    for (int i = 0; i < count; i++)
    {
        if (m_enemyCache[i].IsAlive)
        {
            m_nearbyEnemies.Add(m_enemyCache[i]);
        }
    }
}
```

### Caching Expensive Operations

- ✅ Cache results of expensive calculations outside Update.
- ✅ Use dirty flags to recalculate only when state changes.
- ✅ Cache Transform, Rigidbody, and other component references in Awake().

```csharp
// ✅ Good - cached
private Transform m_transform;
private bool m_isDirty = true;

private void Awake()
{
    m_transform = transform;
}

private void Update()
{
    if (m_isDirty)
    {
        m_cachedValue = ExpensiveCalculation();
        m_isDirty = false;
    }
}
```

### Memory Management

**String Operations:**
- ❌ Never use string concatenation (`+`) in loops.
- ✅ Use `StringBuilder` for dynamic strings.

**Collections:**
- ✅ Initialize collections with expected capacity.
- ✅ Use `.Clear()` instead of creating new lists.
- ✅ Use `ListPool<T>` from Unity's pooling utilities.

### Physics Optimization

- ✅ Use layer masks to limit physics queries.
- ✅ Cache `LayerMask` values.
- ✅ Use non-allocating physics methods: `Physics.RaycastNonAlloc`, `Physics.OverlapSphereNonAlloc`.

```csharp
// ✅ Good - non-allocating
private readonly Collider[] m_hitBuffer = new Collider[32];
private LayerMask m_enemyLayer;

private void Awake()
{
    m_enemyLayer = LayerMask.GetMask("Enemy");
}

private void FixedUpdate()
{
    int hitCount = Physics.OverlapSphereNonAlloc(
        transform.position, m_radius, m_hitBuffer, m_enemyLayer
    );

    for (int i = 0; i < hitCount; i++)
    {
        ProcessHit(m_hitBuffer[i]);
    }
}
```

### Rendering Optimization

- ⚠️ Accessing `.material` creates a material instance — use `.sharedMaterial` when possible.
- ✅ Use `MaterialPropertyBlock` for per-instance changes.

```csharp
// ✅ Good - uses MaterialPropertyBlock
private static readonly int k_colorId = Shader.PropertyToID("_Color");
private MaterialPropertyBlock m_propertyBlock;

private void Awake()
{
    m_propertyBlock = new MaterialPropertyBlock();
}

private void SetColor(Color color)
{
    m_renderer.GetPropertyBlock(m_propertyBlock);
    m_propertyBlock.SetColor(k_colorId, color);
    m_renderer.SetPropertyBlock(m_propertyBlock);
}
```

### GetComponent and Find Operations

- ❌ **Never call GetComponent in Update** — cache in Awake/Start.
- ❌ Avoid `FindObjectOfType`, `FindObjectsOfType` at runtime.
- ✅ Use `[SerializeField]` to assign references in the Inspector.

```csharp
// ✅ Good - cached
private Rigidbody m_rigidbody;

private void Awake()
{
    TryGetComponent(out m_rigidbody);
}
```

### LINQ and Delegates

- ❌ **Never use LINQ in Update loops**.
- ❌ Avoid lambda expressions in hot paths.
- ✅ Use explicit loops instead.

### Async and Coroutine Patterns

- ✅ Prefer `Awaitable` (Unity 6+) over coroutines.
- ✅ Cache `WaitForSeconds` objects when using coroutines repeatedly.

```csharp
// ✅ Better - Unity 6 Awaitable (no allocation)
private async Awaitable PeriodicUpdateAsync(CancellationToken token)
{
    while (!token.IsCancellationRequested)
    {
        await Awaitable.WaitForSecondsAsync(0.1f, token);
        if (this == null) return;
        DoSomething();
    }
}
```

### Profiling Markers

```csharp
using Unity.Profiling;

private static readonly ProfilerMarker s_updateMarker =
    new ProfilerMarker("PerformanceCriticalSystem.Update");

private void Update()
{
    using (s_updateMarker.Auto())
    {
        // Code to profile
    }
}
```

### Common Anti-Patterns

| Anti-Pattern | Impact | Solution |
|--------------|--------|----------|
| `GetComponent` in Update | High | Cache in Awake |
| `FindObjectOfType` at runtime | High | Use references or events |
| `new List<T>()` in Update | High | Pre-allocate and Clear() |
| String concatenation in loops | Medium | Use StringBuilder |
| `Camera.main` in Update | Medium | Cache reference |
| LINQ in Update | Medium | Use explicit loops |

---

## Project Configuration & Editor Settings

### Enter Play Mode Options

**Location:** Edit → Project Settings → Editor

| Setting | Value | Why |
|---------|-------|-----|
| Reload Domain | ❌ Disabled | Faster iteration (saves 2-5 seconds per play) |
| Reload Scene | ❌ Disabled | Keeps scene state |

**Caveat:** Static fields persist. Use `[RuntimeInitializeOnLoadMethod]` to reset statics.

### Asset Pipeline Settings

**Location:** Unity → Settings or Edit → Preferences

| Setting | Value |
|---------|-------|
| Auto Refresh | ❌ Disabled |

**Manual refresh:** `Cmd+R` (macOS) or `Ctrl+R` (Windows).

### Burst Compiler Settings

**Location:** Edit → Project Settings → Burst AOT Settings

| Setting | Value |
|---------|-------|
| Enable Burst | ✅ Enabled |
| Synchronous Compilation | ✅ Enabled |

---

## Common Mistakes & Troubleshooting

### Quick Reference Table

| ❌ Wrong | ✅ Correct | Notes |
|----------|-----------|-------|
| `color: #FF0000;` | `color: rgb(255, 0, 0);` | USS doesn't support hex |
| `binding-path="health"` | `binding-path="Health"` | Case-sensitive |
| `button.onClick += ...` | `button.clicked += ...` | Use `clicked` |
| Missing `[CreateProperty]` | Add attribute | Required for binding |
| Missing `INotifyBindablePropertyChanged` | Implement | Required for reactive updates |
| `root.Q("name")` | `root.Q<VisualElement>("name")` | Always include type |
| Not unsubscribing events | Unsubscribe in `OnDisable` | Memory leaks |
| `navBarMenu` | `navbar-menu` | Use kebab-case |
| `GetComponent` in Update | Cache in Awake | Major performance issue |
| String concatenation in Update | Use StringBuilder | Creates garbage |
| LINQ in Update | Explicit loops | Creates garbage |

### Binding Not Updating

Checklist:
1. ✅ Property has `[CreateProperty]`
2. ✅ Class implements `INotifyBindablePropertyChanged`
3. ✅ `Notify()` is called when the property changes
4. ✅ `dataSource` is assigned in C#
5. ✅ `binding-path` matches property name exactly (case-sensitive)

### Element Not Visible

1. Check `display` is not `None`
2. Check `visibility` is not `Hidden`
3. Check parent has `flex-grow: 1` or explicit size
4. Open the **UI Toolkit Debugger** (Window → UI Toolkit → Debugger)

### Button Not Responding

1. Verify subscription is in `OnEnable`
2. Check not disabled (`button.SetEnabled(false)`)
3. Check no overlay blocking pointer events
4. Check `picking-mode` is `position`

### Query Returns Null

1. Verify `name` attribute is set in UXML
2. Query in `OnEnable`, not `Awake` (for UIDocument)
3. Use correct type (`Q<Button>` not `Q<VisualElement>`)
4. Check for typos — name matching is case-sensitive

---

## Summary: Quick Checklist

### Always Do ✅
- Cache component references in Awake()
- Pre-allocate collections with expected capacity
- Subscribe in OnEnable(), unsubscribe in OnDisable()
- Use `[CreateProperty]` for bindable properties
- Name methods with clear verbs (Process/Handle pattern)
- Use correct prefixes (`m_`, `s_`, `k_`)
- Use non-allocating physics methods
- Cache shader property IDs

### Never Do ❌
- GetComponent/Find in Update loops
- Allocate (new) in Update loops
- Use LINQ in Update loops
- String concatenation in hot paths
- Access `Camera.main` every frame
- Create WaitForSeconds in coroutine loops
- Forget to unsubscribe from events
- Use hex colors in USS
- Forget `[CreateProperty]` or `INotifyBindablePropertyChanged`

### Consider ⚠️
- Throttle expensive operations
- Batch physics queries
- Profile on target platform
- Use Jobs/Burst for heavy computation
- Use object pooling for frequently spawned objects

---

**Document Version:** 2.0
**Last Updated:** February 2026
**Target Platform:** Unity 6.3+
