
# GitHub Copilot Instructions: Unity Input System

Use this cheat sheet for LLM completions. See the readme.md for the general rationale behind these guidelines.

Table of contents:
- [Unity Version-Specific Instructions](#unity-version-specific-instructions)
- [Core Concepts](#core-concepts)
- [InputActionAsset and InputActionReference](#inputactionasset-and-inputactionreference)
- [PlayerInput Component](#playerinput-component)
- [Action Map Patterns](#action-map-patterns)
- [Callback Context and Phases](#callback-context-and-phases)
- [Subscribing and Unsubscribing](#subscribing-and-unsubscribing)
- [Event-Driven vs Polling](#event-driven-vs-polling)
- [Enabling and Disabling Action Maps](#enabling-and-disabling-action-maps)
- [Runtime Rebinding](#runtime-rebinding)
- [Global Input Handling](#global-input-handling)

# Unity Version-Specific Instructions

- ℹ️ This project uses Unity 6.3 and the **New Input System** package. Do not use the legacy `Input` class (`Input.GetKey`, `Input.GetAxis`).
- ℹ️ The New Input System package must be installed via **Package Manager** and the Active Input Handling must be set to **Input System Package (New)** or **Both** in **Project Settings > Player**.
- ℹ️ Unity 6 supports Enhanced Touch API (`EnhancedTouchSupport.Enable()`) for mobile — prefer it over polling `Input.touches`.
- ℹ️ Use `InputSystem.onEvent` for global diagnostics and low-level device handling only.

# Core Concepts

- ✅ Build input around `InputActionAsset` — a serialized asset containing Action Maps and Actions.
- ✅ Reference specific actions via `InputActionReference` fields serialized in the Inspector.
- ✅ Use event-driven callbacks (`started`, `performed`, `cancelled`) rather than polling in `Update`.
- ❌ Never use string-based lookups like `playerInput.actions["Jump"]` in hot paths — cache references.
- ❌ Never use the old `Input` class when the New Input System package is active.

# InputActionAsset and InputActionReference

- ✅ Create a single `InputActionAsset` (`.inputactions` file) per control scheme / player type.
- ✅ Serialize `InputActionReference` fields on MonoBehaviours to reference individual actions without coupling to the asset structure.
- ✅ `InputActionReference` prevents runtime string lookups and survives asset refactoring.
- ❌ Avoid storing a reference to the whole `InputActionAsset` on every MonoBehaviour — centralise it.

```csharp
public class PlayerJump : MonoBehaviour
{
    // Drag the Jump action from the InputActionAsset into this field in the Inspector
    [SerializeField] private InputActionReference m_JumpAction;

    private void OnEnable()
    {
        m_JumpAction.action.performed += HandleJump;
        m_JumpAction.action.Enable();
    }

    private void OnDisable()
    {
        m_JumpAction.action.performed -= HandleJump;
        m_JumpAction.action.Disable();
    }

    private void HandleJump(InputAction.CallbackContext context)
    {
        // context.performed is true here — safe to act
        Debug.Log("Jump performed");
    }
}
```

# PlayerInput Component

- ✅ Use the `PlayerInput` component for single-player games or when you need automatic action map switching.
- ✅ Set **Behavior** to **Invoke Unity Events** for inspector-wired callbacks, or **Invoke C Sharp Events** for code-driven callbacks.
- ✅ Use `PlayerInput.onActionTriggered` (C# Events mode) for a single entry point that dispatches all actions.
- ❌ Avoid **Send Messages** behavior — it relies on reflection and is the slowest option.
- ℹ️ `PlayerInput` automatically enables the **Default Map** specified on the component and switches maps when devices change.

```csharp
// Using PlayerInput with C# Events behavior
public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput m_PlayerInput;

    private void Awake()
    {
        m_PlayerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        m_PlayerInput.onActionTriggered += HandleAction;
    }

    private void OnDisable()
    {
        m_PlayerInput.onActionTriggered -= HandleAction;
    }

    private void HandleAction(InputAction.CallbackContext context)
    {
        switch (context.action.name)
        {
            case "Jump":
                if (context.performed) HandleJump();
                break;
            case "Move":
                if (context.performed || context.canceled) HandleMove(context.ReadValue<Vector2>());
                break;
        }
    }

    private void HandleJump() { /* ... */ }
    private void HandleMove(Vector2 direction) { /* ... */ }
}
```

# Action Map Patterns

- ✅ Organise actions into Action Maps by game state: `Gameplay`, `UI`, `Vehicle`, `Cutscene`.
- ✅ Keep each Action Map focused — avoid a single "Player" map with 30+ actions.
- ✅ Enable exactly one Action Map at a time unless you explicitly need overlapping input (e.g. UI always-on).
- ❌ Avoid enabling all maps simultaneously — it leads to unintended input leakage between states.

```csharp
public class InputMapSwitcher : MonoBehaviour
{
    [SerializeField] private PlayerInput m_PlayerInput;

    public void SwitchToGameplay()
    {
        m_PlayerInput.SwitchCurrentActionMap("Gameplay");
    }

    public void SwitchToUI()
    {
        m_PlayerInput.SwitchCurrentActionMap("UI");
    }
}
```

# Callback Context and Phases

- ℹ️ Every `InputAction` callback receives an `InputAction.CallbackContext` with three phase events:
  - `started` — the interaction has begun (e.g. button pressed down).
  - `performed` — the interaction has completed (e.g. button held long enough, or tap released).
  - `cancelled` — the interaction was interrupted (e.g. button released before hold duration).
- ✅ Check `context.phase` or subscribe to the specific phase event that matches the desired behavior.
- ✅ Use `context.ReadValue<T>()` to read the current value (Vector2 for sticks, float for triggers, etc.).
- ✅ Use `context.performed` for one-shot actions like Jump or Fire.
- ✅ Subscribe to both `performed` and `cancelled` for continuous actions like Move or Aim.

```csharp
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputActionReference m_MoveAction;

    private Vector2 m_MoveInput;

    private void OnEnable()
    {
        m_MoveAction.action.performed += HandleMovePerformed;
        m_MoveAction.action.canceled += HandleMoveCanceled;
        m_MoveAction.action.Enable();
    }

    private void OnDisable()
    {
        m_MoveAction.action.performed -= HandleMovePerformed;
        m_MoveAction.action.canceled -= HandleMoveCanceled;
        m_MoveAction.action.Disable();
    }

    private void HandleMovePerformed(InputAction.CallbackContext context)
    {
        m_MoveInput = context.ReadValue<Vector2>();
    }

    private void HandleMoveCanceled(InputAction.CallbackContext context)
    {
        m_MoveInput = Vector2.zero;
    }

    private void FixedUpdate()
    {
        // Apply cached m_MoveInput to physics — no polling in this method
        ApplyMovement(m_MoveInput);
    }

    private void ApplyMovement(Vector2 direction) { /* ... */ }
}
```

# Subscribing and Unsubscribing

- ✅ Always subscribe in `OnEnable` and unsubscribe in `OnDisable` — not in `Awake`/`Start`/`OnDestroy`.
- ✅ Enable the action in `OnEnable` and disable it in `OnDisable` when managing actions directly (without `PlayerInput`).
- ❌ Never subscribe without unsubscribing — it causes callbacks to fire on destroyed objects and memory leaks.
- ❌ Never enable an action without also disabling it on cleanup.

```csharp
// Correct pattern for direct action management
public class AbilityActivator : MonoBehaviour
{
    [SerializeField] private InputActionReference m_AbilityAction;

    private void OnEnable()
    {
        m_AbilityAction.action.performed += HandleAbility;
        m_AbilityAction.action.Enable();
    }

    private void OnDisable()
    {
        // Unsubscribe before Disable to avoid a callback firing during disable
        m_AbilityAction.action.performed -= HandleAbility;
        m_AbilityAction.action.Disable();
    }

    private void HandleAbility(InputAction.CallbackContext context)
    {
        Debug.Log("Ability activated");
    }
}
```

# Event-Driven vs Polling

- ✅ Use event-driven callbacks as the primary input pattern — store the value and use it in `Update`/`FixedUpdate`.
- ✅ Cache `ReadValue<T>()` results in fields when you need the value every frame (e.g. movement direction).
- ❌ Avoid calling `m_MoveAction.action.ReadValue<Vector2>()` inside `Update` every frame — subscribe to callbacks instead.
- ❌ Avoid `Input.GetKey` / `Input.GetAxis` entirely when the New Input System is active.

```csharp
// AVOID — polling ReadValue in Update
private void Update()
{
    Vector2 move = m_MoveAction.action.ReadValue<Vector2>(); // allocates and polls every frame
    transform.Translate(move * Time.deltaTime);
}

// PREFER — event-driven with cached value
private Vector2 m_CachedMove;

private void HandleMovePerformed(InputAction.CallbackContext ctx) => m_CachedMove = ctx.ReadValue<Vector2>();
private void HandleMoveCanceled(InputAction.CallbackContext ctx) => m_CachedMove = Vector2.zero;

private void Update()
{
    transform.Translate(m_CachedMove * Time.deltaTime);
}
```

# Enabling and Disabling Action Maps

- ✅ Enable/disable action maps at the `InputActionMap` level when not using `PlayerInput`.
- ✅ Store a reference to the generated C# class from the `.inputactions` asset when using the generated wrapper.
- ℹ️ Generating a C# wrapper class from the `.inputactions` asset gives strongly typed access to all actions.

```csharp
// Using the generated C# class from the .inputactions asset
public class GameInputController : MonoBehaviour
{
    private GameInputActions m_InputActions;

    private void Awake()
    {
        m_InputActions = new GameInputActions();
    }

    private void OnEnable()
    {
        m_InputActions.Gameplay.Enable();
        m_InputActions.Gameplay.Jump.performed += HandleJump;
    }

    private void OnDisable()
    {
        m_InputActions.Gameplay.Jump.performed -= HandleJump;
        m_InputActions.Gameplay.Disable();
    }

    private void OnDestroy()
    {
        m_InputActions.Dispose();
    }

    private void HandleJump(InputAction.CallbackContext ctx)
    {
        Debug.Log("Jump");
    }
}
```

# Runtime Rebinding

- ✅ Use `InputAction.PerformInteractiveRebinding()` to let players remap controls at runtime.
- ✅ Save rebinding overrides with `action.SaveBindingOverridesAsJson()` and restore with `action.LoadBindingOverridesFromJson()`.
- ✅ Disable the action before starting rebinding and re-enable it after completion.
- ❌ Never attempt rebinding while the action is enabled — it will throw an exception.

```csharp
public class RebindUI : MonoBehaviour
{
    [SerializeField] private InputActionReference m_ActionToRebind;

    private InputActionRebindingExtensions.RebindingOperation m_RebindOperation;

    public void StartRebind()
    {
        m_ActionToRebind.action.Disable();

        m_RebindOperation = m_ActionToRebind.action.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => CompleteRebind())
            .Start();
    }

    private void CompleteRebind()
    {
        m_RebindOperation.Dispose();
        m_ActionToRebind.action.Enable();

        string json = m_ActionToRebind.action.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("Rebinds", json);
    }

    private void OnDestroy()
    {
        m_RebindOperation?.Dispose();
    }
}
```

# Global Input Handling

- ✅ Use `InputSystem.onEvent` for low-level device diagnostics only (e.g. logging all raw input events).
- ✅ Use `InputUser` for multi-player pairing of devices to users.
- ❌ Do not use `InputSystem.onEvent` as a primary input dispatch mechanism — use Action callbacks instead.

```csharp
// Diagnostic-only usage of InputSystem.onEvent
private void OnEnable()
{
    InputSystem.onEvent += HandleRawInputEvent;
}

private void OnDisable()
{
    InputSystem.onEvent -= HandleRawInputEvent;
}

private void HandleRawInputEvent(InputEventPtr eventPtr, InputDevice device)
{
    // Only for diagnostics — log which device is sending events
    Debug.Log($"[Input] Event from device: {device.displayName}");
}
```
