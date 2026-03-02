
# GitHub Copilot Instructions: Unity Assembly Definitions

Use this cheat sheet for LLM completions. See the readme.md for the general rationale behind these guidelines.

Table of contents:
- [Unity Version-Specific Instructions](#unity-version-specific-instructions)
- [Why Use Assembly Definitions](#why-use-assembly-definitions)
- [When NOT to Use Assembly Definitions](#when-not-to-use-assembly-definitions)
- [Naming Conventions](#naming-conventions)
- [Recommended Project Assembly Structure](#recommended-project-assembly-structure)
- [Runtime Assembly](#runtime-assembly)
- [Editor Assembly](#editor-assembly)
- [Test Assemblies](#test-assemblies)
- [Dependency Graph Rules](#dependency-graph-rules)
- [Avoiding Circular References](#avoiding-circular-references)
- [Editor-Only Assemblies](#editor-only-assemblies)
- [Overriding Precompiled DLL References](#overriding-precompiled-dll-references)
- [rootNamespace Field](#rootnamespace-field)

# Unity Version-Specific Instructions

- ℹ️ This project uses Unity 6.3. Assembly Definition Files (`.asmdef`) are supported since Unity 2017.3 and the format is stable in Unity 6.
- ℹ️ Unity 6 compiles assemblies using Roslyn — the compilation pipeline is unchanged, but assembly isolation becomes even more important as project scale grows.
- ℹ️ Unity 6 supports the `rootNamespace` field in `.asmdef` — always set it to match the namespace used by scripts in that assembly.
- ℹ️ The Unity Editor assembly (`Assembly-CSharp-Editor`) and the main assembly (`Assembly-CSharp`) still exist for scripts not covered by any `.asmdef` — avoid leaving scripts in these fallback assemblies.

# Why Use Assembly Definitions

- ✅ Assembly definitions compile only the assemblies that have changed — a single script change in an isolated assembly does not recompile the entire project.
- ✅ Assembly isolation enforces architectural boundaries at compile time — an accidental reference from runtime code to editor-only code becomes a compile error.
- ✅ Test assemblies are excluded from builds automatically — no `#if UNITY_EDITOR` guards needed around test code.
- ✅ Dependency direction is explicit and machine-checkable — you can see the full dependency graph.

# When NOT to Use Assembly Definitions

- ❌ Do not add `.asmdef` files to very small projects (< 10 scripts) — the overhead of managing dependencies outweighs the compilation benefit.
- ❌ Do not create an assembly per script or per folder at fine granularity — group logically related systems into one assembly.
- ❌ Do not use assemblies purely to organise folders — namespaces already provide that; assemblies are for compilation boundaries.
- ℹ️ A practical threshold: consider adding `.asmdef` files when total script compilation time exceeds 5 seconds or when you need to enforce architectural rules.

# Naming Conventions

- ✅ Assembly name matches the root namespace of the scripts it contains.
- ✅ Format: `{Company}.{Project}.{Domain}` — e.g. `Acme.SpaceGame.Gameplay`.
- ✅ Editor assemblies append `.Editor`: `Acme.SpaceGame.Gameplay.Editor`.
- ✅ Test assemblies append `.Tests` plus the mode: `Acme.SpaceGame.Gameplay.EditModeTests`, `Acme.SpaceGame.Gameplay.PlayModeTests`.
- ✅ The `.asmdef` filename matches the assembly name field exactly.
- ❌ Never use generic names like `Scripts`, `GameLogic`, or `Utilities` without a namespace prefix.

# Recommended Project Assembly Structure

```
Assets/
  _Project/
    Scripts/
      Core/
        Acme.SpaceGame.Core.asmdef              ← shared types, interfaces
      Gameplay/
        Acme.SpaceGame.Gameplay.asmdef          ← depends on Core
      UI/
        Acme.SpaceGame.UI.asmdef                ← depends on Core, Gameplay
      Editor/
        Acme.SpaceGame.Editor.asmdef            ← editor tools, depends on Core (Editor-only)
      Tests/
        EditMode/
          Acme.SpaceGame.Core.EditModeTests.asmdef
          Acme.SpaceGame.Gameplay.EditModeTests.asmdef
        PlayMode/
          Acme.SpaceGame.Gameplay.PlayModeTests.asmdef
```

Dependency arrows (read: "depends on"):

```
UI  ──►  Gameplay  ──►  Core
Editor   ──►  Core
Tests    ──►  (assembly under test)
```

# Runtime Assembly

`Acme.SpaceGame.Gameplay.asmdef`:

```json
{
    "name": "Acme.SpaceGame.Gameplay",
    "rootNamespace": "Acme.SpaceGame.Gameplay",
    "references": [
        "Acme.SpaceGame.Core"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- ℹ️ `"includePlatforms": []` means all platforms — omit or leave empty for runtime assemblies.
- ℹ️ `"autoReferenced": true` allows other assemblies to reference this one without explicit declaration (set to `false` for strict control).
- ✅ Set `"autoReferenced": false` for domain-specific assemblies that should only be referenced explicitly.

# Editor Assembly

`Acme.SpaceGame.Editor.asmdef`:

```json
{
    "name": "Acme.SpaceGame.Editor",
    "rootNamespace": "Acme.SpaceGame.Editor",
    "references": [
        "Acme.SpaceGame.Core",
        "Acme.SpaceGame.Gameplay"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": false,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- ✅ Always set `"includePlatforms": ["Editor"]` for editor-only assemblies.
- ✅ Editor assemblies may reference runtime assemblies — but runtime assemblies must never reference editor assemblies.
- ❌ Never reference an editor assembly from a runtime assembly — it breaks standalone builds.

# Test Assemblies

EditMode test assembly (`Acme.SpaceGame.Gameplay.EditModeTests.asmdef`):

```json
{
    "name": "Acme.SpaceGame.Gameplay.EditModeTests",
    "rootNamespace": "Acme.SpaceGame.Gameplay.Tests",
    "references": [
        "Acme.SpaceGame.Core",
        "Acme.SpaceGame.Gameplay",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [],
    "versionDefines": [],
    "testPlatforms": [
        "EditMode"
    ]
}
```

PlayMode test assembly (`Acme.SpaceGame.Gameplay.PlayModeTests.asmdef`):

```json
{
    "name": "Acme.SpaceGame.Gameplay.PlayModeTests",
    "rootNamespace": "Acme.SpaceGame.Gameplay.Tests",
    "references": [
        "Acme.SpaceGame.Core",
        "Acme.SpaceGame.Gameplay",
        "UnityEngine.TestRunner"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": false,
    "defineConstraints": [],
    "versionDefines": [],
    "testPlatforms": [
        "PlayMode"
    ]
}
```

- ✅ Set `"testPlatforms"` to restrict the assembly to the correct test mode — this excludes it from builds automatically.
- ✅ Set `"autoReferenced": false` on all test assemblies — they should never be referenced by production code.
- ✅ Set `"overrideReferences": true` and list `"nunit.framework.dll"` explicitly in EditMode test assemblies.

# Dependency Graph Rules

- ✅ Dependencies must be **directed and acyclic** — A may reference B, but B must not reference A.
- ✅ Only reference assemblies that are actually needed — do not add references speculatively.
- ✅ Shared types (interfaces, data structures) belong in a `Core` or `Shared` assembly at the bottom of the dependency graph.
- ❌ Never create circular references — Unity will fail to compile with a circular dependency error.
- ❌ Never reference `Assembly-CSharp` (the fallback assembly) from a named `.asmdef` assembly — scripts in the fallback assembly already see all named assemblies if `autoReferenced` is true.

```
VALID dependency direction:
UI ──► Gameplay ──► Core
            ▲
         Physics ──► Core

INVALID (circular):
Gameplay ──► UI ──► Gameplay  (compile error)
```

# Avoiding Circular References

Circular references are the most common `.asmdef` pitfall. The fix is always to extract shared types into a lower-level assembly.

```
Problem:
  PlayerSystem needs IEventBus from EventSystem
  EventSystem needs PlayerState from PlayerSystem
  → Circular reference

Solution: Extract shared types
  SharedInterfaces.asmdef (IEventBus, PlayerState)
  PlayerSystem.asmdef → SharedInterfaces
  EventSystem.asmdef  → SharedInterfaces
```

- ✅ If two assemblies need to reference each other, extract the shared contract (interfaces, data transfer objects) into a third assembly that both depend on.
- ✅ Use interfaces rather than concrete types in inter-assembly APIs — this minimises coupling.

# Editor-Only Assemblies

- ✅ Any code that uses `UnityEditor` namespace must live in an editor-only assembly (`"includePlatforms": ["Editor"]`).
- ✅ Custom inspectors, property drawers, editor windows, and menu items must be in an editor-only assembly.
- ✅ `#if UNITY_EDITOR` guards inside a runtime assembly are a code smell — extract that code to a proper editor assembly instead.
- ❌ Never put `using UnityEditor;` in a script inside a runtime assembly — the build will fail on non-editor platforms.

```csharp
// This code belongs in an editor assembly (includePlatforms: ["Editor"])
// NOT in a runtime assembly with #if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HealthSystem))]
public class HealthSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Reset Health"))
        {
            ((HealthSystem)target).ResetHealth();
        }
    }
}
```

# Overriding Precompiled DLL References

- ✅ Use `"overrideReferences": true` when you need to reference a specific precompiled `.dll` not managed by Unity's package system.
- ✅ List the `.dll` filenames in `"precompiledReferences"` — place the DLLs in the `Plugins/` folder.
- ❌ Do not use `overrideReferences` unless you have a concrete reason — it disables automatic reference resolution for that assembly.

```json
{
    "name": "Acme.SpaceGame.Serialization",
    "rootNamespace": "Acme.SpaceGame.Serialization",
    "references": [
        "Acme.SpaceGame.Core"
    ],
    "overrideReferences": true,
    "precompiledReferences": [
        "Newtonsoft.Json.dll",
        "MessagePack.dll"
    ],
    "autoReferenced": false
}
```

# rootNamespace Field

- ✅ Always set `rootNamespace` to the namespace prefix shared by all scripts in the assembly.
- ✅ `rootNamespace` is used by Unity's script template generator to pre-fill the `namespace` declaration when creating new scripts inside the assembly folder.
- ❌ Do not leave `rootNamespace` empty — scripts created in the folder will not have a namespace pre-filled.
- ❌ Do not set `rootNamespace` to a namespace that differs from the actual namespaces used in the scripts — it causes confusion.

```json
{
    "name": "Acme.SpaceGame.Gameplay",
    "rootNamespace": "Acme.SpaceGame.Gameplay"
}
```

All `.cs` scripts in this assembly should open with:

```csharp
namespace Acme.SpaceGame.Gameplay
{
    // ...
}
```
