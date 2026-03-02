# Custom Script Templates: Creating Your Own in Unity 6

When you create a new C# script in Unity, the Editor generates a file from a built-in template.
Customizing these templates lets you enforce your team's code style from the first line,
eliminating repetitive boilerplate cleanup on every new script.

## Overview

Unity has supported custom script templates for years, but the process has changed across versions.
Older guides point you to the Editor's installation folder deep inside the `.app` bundle (macOS)
or `Program Files` (Windows).

That approach still works but has real drawbacks: templates are overwritten on every Unity update,
require elevated permissions to edit, and aren't shared with your team through version control.

Instead you can use **project-level templates** via an `Assets/ScriptTemplates/` folder.
This is the recommended approach for Unity 6 and later.
Templates placed here override the built-in defaults, travel with your repository, and require no special permissions.

## Step 1: Create the ScriptTemplates folder

Create a folder named `ScriptTemplates` as a **direct child** of your `Assets/` folder.
The name and location are exact requirements. Unity will not detect templates placed anywhere else.

```
Assets/
  ScriptTemplates/       <-- Must be exactly here
    ...template files
  Scripts/
  Scenes/
```

> **Important:** Do not place `ScriptTemplates` inside a subfolder like `Assets/Editor/ScriptTemplates/`. It must be at `Assets/ScriptTemplates/`.

## Step 2: Understand the file naming convention

Template file names control how they appear in the **Create** context menu (right-click in the Project window). The naming format is:

```
{Priority}-{MenuPath}-{DefaultFileName}.{extension}.txt
```

Each part serves a specific purpose:

| Part | Purpose | Example |
|------|---------|---------|
| Priority | Sort order in the menu (lower = higher) | `1`, `2`, `3` |
| MenuPath | Menu and submenu names, separated by `__` | `Scripting__MonoBehaviour Script` |
| DefaultFileName | Pre-filled name when creating the asset | `NewMonoBehaviourScript` |
| Extension | File extension of the generated script | `.cs` |
| `.txt` suffix | Required so Unity treats it as a template, not a script | Always `.txt` |

The double underscore (`__`) acts as a submenu separator. For example:

```
1-Scripting__MonoBehaviour Script-NewMonoBehaviourScript.cs.txt
```

This creates the menu item: **Create > Scripting > MonoBehaviour Script** with a default file name of `NewMonoBehaviourScript.cs`.

### Built-in templates you can override

These are the default template file names from Unity 6.3. Use the same names to replace them:

| File Name | Menu Item |
|-----------|-----------|
| `1-Scripting__MonoBehaviour Script-NewMonoBehaviourScript.cs.txt` | MonoBehaviour Script |
| `2-Scripting__ScriptableObject Script-NewScriptableObjectScript.cs.txt` | ScriptableObject Script |
| `3-Scripting__Empty C# Script-NewEmptyCSharpScript.cs.txt` | Empty C# Script |

You can also add entirely new templates with custom names. Use a unique priority number and menu path to create additional menu entries.

## Step 3: Write your template content

Template files are plain text with special keyword placeholders that Unity replaces at creation time.

### Available keywords

| Keyword | Replaced With |
|---------|---------------|
| `#SCRIPTNAME#` | The file name you type when creating the script |
| `#ROOTNAMESPACEBEGIN#` | Opening namespace declaration (if set in Project Settings > Editor > Root Namespace) |
| `#ROOTNAMESPACEEND#` | Closing namespace brace |
| `#NOTRIM#` | Invisible marker that preserves trailing whitespace on that line |

### Example: MonoBehaviour template

This project's MonoBehaviour template scaffolds the class in Unity execution order with regions for organization:

```csharp
using UnityEngine;

    #ROOTNAMESPACEBEGIN#
public class #SCRIPTNAME# : MonoBehaviour
{
    #region Fields

    #endregion

    #region Properties

    #endregion

    #region Events

    #endregion

    #region MonoBehaviour Methods

    private void Awake()
    {
        // Cache component references here
        #NOTRIM#
    }

    private void OnEnable()
    {
        // Subscribe to events here
        #NOTRIM#
    }

    private void Start()
    {
        // Initialization that depends on other components being ready
        #NOTRIM#
    }

    private void OnDisable()
    {
        // Unsubscribe from events here
        #NOTRIM#
    }

    private void Update()
    {
        #NOTRIM#
    }

    #endregion

    #region Public Methods

    #endregion

    #region Private Methods

    #endregion
}
#ROOTNAMESPACEEND#
```

Compare this to Unity's built-in default, which only includes `Start()` and `Update()` with no structure:

```csharp
using UnityEngine;

public class #SCRIPTNAME# : MonoBehaviour
{
    void Start()
    {
        #NOTRIM#
    }

    void Update()
    {
        #NOTRIM#
    }
}
```

### Example: ScriptableObject template

```csharp
using UnityEngine;

    #ROOTNAMESPACEBEGIN#
/// <summary>
/// Static configuration data. Do not use for runtime state.
/// </summary>
[CreateAssetMenu(fileName = "#SCRIPTNAME#", menuName = "Scriptable Objects/#SCRIPTNAME#")]
public class #SCRIPTNAME# : ScriptableObject
{
    #region Fields

    #endregion

    #region Properties

    #endregion
}
#ROOTNAMESPACEEND#
```

### Namespace handling

The `#ROOTNAMESPACEBEGIN#` and `#ROOTNAMESPACEEND#` keywords are controlled by the **Root Namespace** field in **Project Settings > Editor**. If a root namespace is set (e.g., `MyGame`), Unity wraps the generated code in a namespace block. If the field is empty, the keywords are stripped out and no namespace is added.

The indentation before `#ROOTNAMESPACEBEGIN#` controls the indent level of the namespace content. Unity uses this whitespace to properly format the generated file.

## Step 4: Restart the Unity Editor

After adding or modifying templates, you must **close and reopen the Unity Editor**. Unity reads the `ScriptTemplates` folder at startup and wires the file names into the Create menu at that time. Changes will not appear until you restart.

## Step 5: Verify your templates

1. Right-click in the **Project** window.
2. Navigate to **Create > Scripting**.
3. Select your template (e.g., **MonoBehaviour Script**).
4. Name your new script and press Enter.
5. Open the generated file and confirm it matches your template.

## Key Takeaways

| Feature | Value |
|---------|-------|
| Template location | `Assets/ScriptTemplates/` (direct child of Assets) |
| File format | `{Priority}-{MenuPath}-{DefaultName}.{ext}.txt` |
| Submenu separator | Double underscore `__` |
| Key placeholder | `#SCRIPTNAME#` for the file name |
| Namespace support | `#ROOTNAMESPACEBEGIN#` / `#ROOTNAMESPACEEND#` |
| Activation | Requires Editor restart |
| Version control | Committed with the project, shared with the team |

## When to Use

- ✅ Enforce consistent code structure across a team without relying on manual cleanup
- ✅ Pre-populate lifecycle methods in the correct Unity execution order
- ✅ Include project-specific attributes like `[CreateAssetMenu]` on ScriptableObjects
- ✅ Add regions, comments, or using statements your project always needs
- ❌ Avoid editing templates inside the Unity Editor installation folder; they are overwritten on every update
- ❌ Avoid overly complex templates; keep them as a starting point, not a finished script

## Learn More

For more details on the Unity Editor and project configuration:

- [Unity 6 Manual: Project Settings](https://docs.unity3d.com/6000.3/Documentation/Manual/comp-ManagerGroup.html)
- [Unity 6 Manual: Creating and Using Scripts](https://docs.unity3d.com/6000.3/Documentation/Manual/creating-and-using-scripts.html)

## UI Toolkit View Template
  Features:
    - Inherits from UITKBaseClass (your project's base class for all UI views)
    - Includes the Core.UI namespace automatically
    - Pre-implements all required abstract methods:
        - InitializeElements() - for querying UXML elements
        - RegisterCallbacks() - for event subscriptions
        - UnregisterCallbacks() - for cleanup
        - ShowPanel(bool) - for visibility toggling
    - Follows your project's naming conventions and region structure

  How to use it:
    1. Right-click in your Assets folder → Create → C# Script → "UI Toolkit View"
    2. This will now be the 4th option in the script creation menu
    3. Unity will auto-generate the .meta file when you refresh the project

## New Templates in v1.1.0

Eight additional templates are included in this release, covering editor tooling, testing, events, and pooling patterns.

| Priority | Menu Path | Default Filename | Purpose |
|----------|-----------|-----------------|---------|
| 5 | Create > Scripting > Interface | `NewInterface.cs` | Scaffolds a public C# interface prefixed with `I`. Includes a summary doc comment and a placeholder `Execute()` method defining the contract. No using statements — interfaces rarely need them. |
| 6 | Create > Scripting > Custom Editor | `NewCustomEditor.cs` | Scaffolds an `Editor` subclass decorated with `[CustomEditor(typeof(TargetClass))]`. Includes `OnEnable` for property lookup and `OnInspectorGUI` calling `serializedObject.Update()` / `ApplyModifiedProperties()` around `DrawDefaultInspector()`. Replace `TargetClass` with the actual type. |
| 7 | Create > Scripting > Editor Window | `NewEditorWindow.cs` | Scaffolds an `EditorWindow` subclass with a `[MenuItem]` entry under `Window/`, `ShowWindow()`, and `OnGUI()` containing a bold label. Replace the menu path if you want it somewhere other than `Window/`. |
| 8 | Create > Scripting > Property Drawer | `NewPropertyDrawer.cs` | Scaffolds a `PropertyDrawer` subclass decorated with `[CustomPropertyDrawer(typeof(TargetAttribute))]`. Implements both `OnGUI` (with `BeginProperty`/`EndProperty`) and `GetPropertyHeight`. Replace `TargetAttribute` with the attribute class to target. |
| 9 | Create > Scripting > Unit Test EditMode | `NewEditModeTest.cs` | Scaffolds an NUnit `[TestFixture]` class with `[SetUp]` / `[TearDown]` methods and one example `[Test]` using the Arrange–Act–Assert comment pattern. Intended for tests that run without entering Play Mode. |
| 10 | Create > Scripting > Unit Test PlayMode | `NewPlayModeTest.cs` | Scaffolds an NUnit `[TestFixture]` class with `[SetUp]` / `[TearDown]` methods and one example `[UnityTest]` coroutine that yields a frame before asserting. Intended for tests that require the Unity runtime (physics, time, scene objects). |
| 11 | Create > Scripting > Static Event | `NewStaticEvent.cs` | Scaffolds a `static` class acting as a lightweight event bus. Exposes a `static event Action OnEventRaised` and a `static void Raise()` helper. Subscribe in `OnEnable`, unsubscribe in `OnDisable` on each listener. No MonoBehaviour dependency. |
| 12 | Create > Scripting > Object Pool Handler | `NewObjectPoolHandler.cs` | Scaffolds a `MonoBehaviour` that owns a `UnityEngine.Pool.ObjectPool<GameObject>`. Configures the pool in `Awake` with serialized capacity and max-size fields. Implements all four pool callbacks: `CreatePooledItem`, `OnTakeFromPool`, `OnReturnedToPool`, and `OnDestroyPoolObject`. Exposes the pool via a read-only `Pool` property. |
