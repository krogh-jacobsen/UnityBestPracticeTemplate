# Unity UI Toolkit — Create UI

You are creating Unity UI Toolkit UI for **Unity 6.3 exclusively**. Follow all conventions in `UnityUIToolkitInstructions.md`. Never use legacy UGUI (Canvas/GameObject-based UI).

---

## What to deliver

For every UI task, produce **all required files**:

| File | When required |
|---|---|
| `*.uxml` | Always — defines the element hierarchy |
| `*.uss` | Always — defines all styling (no inline styles except layout one-offs) |
| `*.cs` (View) | When user interaction or data display is needed |
| `*.cs` (Model) | When reactive data binding is needed |
| `*.cs` (Presenter) | When game logic drives the UI |

State which files you are creating and where they should go (e.g., `Assets/UI/UXML/`, `Assets/UI/USS/`, `Assets/Scripts/UI/`).

---

## Rules — apply to every file you generate

### UXML

- Namespace declaration: `xmlns:ui="UnityEngine.UIElements"`
- File-level stylesheet: `<Style src="project://database/Assets/UI/Styles/YourFile.uss" />`
- Element `name` attributes: **kebab-case**, unique within their block — used for C# queries
- Element `class` attributes: **BEM** (`block__element--modifier`)
- Files: **PascalCase** (`MainMenu.uxml`, `InventoryPanel.uxml`)
- Data binding: use `binding-path` and `data-source-type` when a data source exists

### USS

- **No hex colours** — use `rgb()` or `rgba()` only
- **No `calc()`**, no `em`/`rem`/`vw`/`vh`, no `display: grid`, no `gap`
- Spacing between flex children: use `margin` on children, not `gap`
- Variables declared in `:root {}` only
- CSS text properties use Unity prefixes: `-unity-text-align`, `-unity-font-style`
- Transform shorthand not supported — use `scale:`, `rotate:`, `translate:` individually
- Pseudo-classes supported: `:hover`, `:active`, `:focus`, `:disabled`, `:checked`, `:selected`
- Pseudo-classes NOT supported: `:nth-child()`, `:not()`, `:first-child`, `:last-child`

### C# naming (follows `UnityCodeStyleInstructions.md`)

- Private instance fields: `m_` prefix
- Static fields: `s_` prefix
- Constants: `k_` prefix
- Selector string constants: `private const string k_myElement = "my-element";`
- Query all elements in `OnEnable`, not `Awake` or `Update`
- Unsubscribe all events in `OnDisable`
- Use `EventRegistry` for bulk cleanup when registering multiple callbacks

### Architecture

- **View** (`*View.cs`): inherits `UITKBaseClass`, queries elements, subscribes to events, no business logic
- **Model** (`*SO.cs` or plain class): implements `INotifyBindablePropertyChanged` + `[CreateProperty]` for reactive binding
- **Presenter** (`*Presenter.cs` or `*Controller.cs`): modifies the model, contains game logic
- Custom elements: use `[UxmlElement]` / `[UxmlAttribute]` — never `UxmlFactory`/`UxmlTraits`

### Show/Hide

Always set both `display` and `pickingMode` when hiding panels:

```csharp
panel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
panel.pickingMode  = show ? PickingMode.Position : PickingMode.Ignore;
```

---

## Checklist before outputting files

- [ ] All colours use `rgb()` / `rgba()` — no hex
- [ ] No `gap` property — child `margin` used instead
- [ ] `name` attributes are kebab-case
- [ ] `class` attributes follow BEM
- [ ] USS file is referenced in UXML via `<Style src="..." />`
- [ ] All C# field names have the correct prefix (`m_`, `s_`, `k_`)
- [ ] Element queries are in `OnEnable`
- [ ] Events are unsubscribed in `OnDisable`
- [ ] No `UxmlFactory` / `UxmlTraits` (Unity 6 deprecated)
- [ ] No `SerializedObject.Bind()` for runtime UI
- [ ] Reactive model implements `INotifyBindablePropertyChanged` + `[CreateProperty]`
- [ ] Any panel `ShowPanel()` sets both `display` and `pickingMode`

---

## Output format

For each file, output:

```
### `Assets/UI/UXML/MyPanel.uxml`
```xml
... file contents ...
```

### `Assets/UI/USS/MyPanel.uss`
```css
... file contents ...
```

### `Assets/Scripts/UI/MyPanelView.cs`
```csharp
... file contents ...
```
```

End with a brief **Setup notes** section listing:
- Any Inspector references to wire up
- Any Preset Manager or UIDocument configuration needed
- Any ScriptableObject assets to create

---

## Usage

**Claude Code:**
```
/project:UnityUIToolkitCreate
```
Then describe the UI you need: `Create a settings panel with volume slider, graphics quality dropdown, and a close button.`

**GitHub Copilot Chat:**
```
@workspace using @UnityUIToolkitCreate.md, create a [describe your UI here]
```

**Any chat interface:**
Reference `UnityUIToolkitInstructions.md` and paste this prompt:
> "Create Unity UI Toolkit UI (UXML + USS + C# View) for [description]. Follow Unity 6.3 conventions: BEM class names, rgb() colours only, no gap property, OnEnable queries, OnDisable cleanup."
