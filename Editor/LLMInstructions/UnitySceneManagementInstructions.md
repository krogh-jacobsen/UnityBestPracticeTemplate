
# GitHub Copilot Instructions: Unity Scene Management

Use this cheat sheet for LLM completions. See the readme.md for the general rationale behind these guidelines.

Table of contents:
- [Unity Version-Specific Instructions](#unity-version-specific-instructions)
- [Core Concepts](#core-concepts)
- [Loading Scenes Asynchronously](#loading-scenes-asynchronously)
- [Additive Scene Loading](#additive-scene-loading)
- [Persistent Scenes and DontDestroyOnLoad](#persistent-scenes-and-dontdestroyonload)
- [Scene Events](#scene-events)
- [Unloading Scenes](#unloading-scenes)
- [Scene-Scoped vs Global Services](#scene-scoped-vs-global-services)
- [Avoiding FindObjectOfType Across Scenes](#avoiding-findobjectoftype-across-scenes)
- [Loading Screen Pattern with Additive Loading](#loading-screen-pattern-with-additive-loading)

# Unity Version-Specific Instructions

- ℹ️ This project uses Unity 6.3. Prefer `await Awaitable` patterns over coroutines for async scene loading.
- ℹ️ Unity 6 introduces `SceneManager.LoadSceneAsync` returning an `AsyncOperation` — this is unchanged from Unity 2022, but can be awaited via `.ToAwaitable()` (Unity 6 extension).
- ℹ️ `Physics.simulationMode` and other subsystems reset on scene unload — account for this in service initialization.
- ℹ️ Unity 6 `SceneManager` API is in `UnityEngine.SceneManagement` — always include this namespace.

# Core Concepts

- ✅ Always load scenes asynchronously — `SceneManager.LoadScene` (synchronous) stalls the main thread and causes a frame spike.
- ✅ Use `LoadSceneMode.Additive` for multi-scene architectures (gameplay + UI + persistent).
- ✅ Use a single **persistent scene** that is never unloaded to host global services, managers, and `DontDestroyOnLoad` objects.
- ❌ Never call `SceneManager.LoadScene` (synchronous) in production — always use `LoadSceneAsync`.
- ❌ Never use `FindObjectOfType<T>()` to locate services that exist in another scene — use a service locator or dependency injection instead.

# Loading Scenes Asynchronously

- ✅ Store the `AsyncOperation` returned by `LoadSceneAsync` to track progress and control activation.
- ✅ Set `asyncOperation.allowSceneActivation = false` to delay scene activation until resources are ready (e.g. until a loading screen is shown).
- ✅ Set `allowSceneActivation = true` only when `asyncOperation.progress >= 0.9f` — Unity loads to 90% then waits for activation.
- ❌ Never assume the scene is ready immediately after calling `LoadSceneAsync` — always await or check `isDone`.

```csharp
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public async Awaitable LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        if (operation == null)
        {
            Debug.LogError($"[SceneLoader] Scene not found in build settings: {sceneName}");
            return;
        }

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            Debug.Log($"[SceneLoader] Loading {sceneName}: {progress:P0}");
            await Awaitable.NextFrameAsync();
        }

        Debug.Log($"[SceneLoader] Scene loaded: {sceneName}");
    }
}
```

# Additive Scene Loading

- ✅ Use `LoadSceneMode.Additive` to load multiple scenes simultaneously without unloading the current one.
- ✅ Keep a `Scene` reference to the loaded scene — you need it for `UnloadSceneAsync` later.
- ✅ Set the active scene explicitly with `SceneManager.SetActiveScene` after additive loading if physics/lighting must apply to the new scene.
- ❌ Never additive-load a scene that contains a `Camera` without managing which camera is active — multiple cameras render simultaneously.

```csharp
public class AdditiveSceneManager : MonoBehaviour
{
    private Scene m_LoadedGameplayScene;

    public async Awaitable LoadGameplaySceneAdditive(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        await operation; // Unity 6 supports awaiting AsyncOperation directly

        m_LoadedGameplayScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(m_LoadedGameplayScene);

        Debug.Log($"[SceneManager] Additively loaded: {sceneName}");
    }

    public async Awaitable UnloadGameplayScene()
    {
        if (!m_LoadedGameplayScene.isLoaded)
            return;

        await SceneManager.UnloadSceneAsync(m_LoadedGameplayScene);
        Debug.Log("[SceneManager] Gameplay scene unloaded.");
    }
}
```

# Persistent Scenes and DontDestroyOnLoad

- ✅ Create a **Bootstrap** or **Persistent** scene that loads first and is never unloaded.
- ✅ Place global singletons (AudioManager, InputManager, AnalyticsService) in the persistent scene.
- ✅ Use `DontDestroyOnLoad(gameObject)` only in the persistent scene — not in gameplay scenes.
- ❌ Avoid `DontDestroyOnLoad` for objects that should reset between levels — use scene-scoped instantiation instead.
- ❌ Never call `DontDestroyOnLoad` on an object that is a child of another `GameObject` — it only works on root objects.

```csharp
// Singleton MonoBehaviour that persists across scenes — lives in the Bootstrap scene
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
```

# Scene Events

- ✅ Subscribe to `SceneManager.sceneLoaded` to react when any scene finishes loading.
- ✅ Subscribe to `SceneManager.sceneUnloaded` to clean up resources tied to a scene.
- ✅ Subscribe to `SceneManager.activeSceneChanged` to update systems that depend on the active scene.
- ✅ Unsubscribe from all `SceneManager` events in `OnDestroy` — static events persist across scene loads.
- ❌ Never forget to unsubscribe from `SceneManager` static events — they keep `MonoBehaviour` instances alive and cause memory leaks.

```csharp
public class SceneEventListener : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        SceneManager.sceneUnloaded += HandleSceneUnloaded;
        SceneManager.activeSceneChanged += HandleActiveSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneUnloaded -= HandleSceneUnloaded;
        SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[Scene] Loaded: {scene.name} ({mode})");
    }

    private void HandleSceneUnloaded(Scene scene)
    {
        Debug.Log($"[Scene] Unloaded: {scene.name}");
    }

    private void HandleActiveSceneChanged(Scene previous, Scene current)
    {
        Debug.Log($"[Scene] Active changed from {previous.name} to {current.name}");
    }
}
```

# Unloading Scenes

- ✅ Use `SceneManager.UnloadSceneAsync(scene)` to unload an additively loaded scene.
- ✅ Await the `AsyncOperation` returned by `UnloadSceneAsync` before loading the next scene to prevent overlapping content.
- ✅ Call `Resources.UnloadUnusedAssets()` after unloading large scenes to reclaim memory immediately.
- ❌ Never try to unload the only active scene — Unity requires at least one scene to be loaded.
- ❌ Never unload a scene while objects in it are still in use — null reference exceptions will follow.

```csharp
public async Awaitable UnloadSceneSafe(string sceneName)
{
    Scene scene = SceneManager.GetSceneByName(sceneName);

    if (!scene.IsValid() || !scene.isLoaded)
    {
        Debug.LogWarning($"[SceneLoader] Scene not loaded: {sceneName}");
        return;
    }

    await SceneManager.UnloadSceneAsync(scene);
    await Resources.UnloadUnusedAssets();

    Debug.Log($"[SceneLoader] Unloaded and cleaned up: {sceneName}");
}
```

# Scene-Scoped vs Global Services

- ✅ Place services that reset between levels (enemy spawner, wave manager, score tracker) in the gameplay scene — let them be destroyed on scene unload.
- ✅ Place services that persist between levels (audio, analytics, input) in the persistent/bootstrap scene.
- ❌ Do not `DontDestroyOnLoad` everything — it creates a growing collection of stale managers across the app's lifetime.

| Service | Scope | Location |
|---|---|---|
| AudioManager | Global | Bootstrap scene |
| InputManager | Global | Bootstrap scene |
| Analytics | Global | Bootstrap scene |
| EnemySpawner | Scene | Gameplay scene |
| ScoreTracker | Scene | Gameplay scene |
| UIHUDController | Scene | UI scene (additive) |

# Avoiding FindObjectOfType Across Scenes

- ❌ Never use `FindObjectOfType<T>()` to locate a service from another scene — it is slow and will return null if the service scene is not loaded.
- ✅ Use a static `Instance` property on a singleton MonoBehaviour (only for genuinely global services).
- ✅ Use a `ScriptableObject`-based service locator to decouple scene-to-scene references.
- ✅ Use dependency injection via constructor parameters or `[SerializeField]` wired in the Inspector for everything else.

```csharp
// ScriptableObject service locator — no scene dependency
[CreateAssetMenu(fileName = "ServiceLocator", menuName = "Services/ServiceLocator")]
public class ServiceLocatorSO : ScriptableObject
{
    [SerializeField] private AudioManager m_AudioManager;
    [SerializeField] private InputManager m_InputManager;

    public AudioManager Audio => m_AudioManager;
    public InputManager Input => m_InputManager;
}
```

# Loading Screen Pattern with Additive Loading

This pattern uses three scenes:
- `Bootstrap` — persistent, never unloaded, hosts global services.
- `LoadingScreen` — loaded additively to display progress, then unloaded.
- `Gameplay` — the target scene.

```csharp
public class GameSceneTransitioner : MonoBehaviour
{
    private const string k_LoadingScreenScene = "LoadingScreen";

    [SerializeField] private LoadingScreenController m_LoadingScreen;

    public async Awaitable TransitionToScene(string targetSceneName)
    {
        // Step 1: Load the loading screen additively
        await SceneManager.LoadSceneAsync(k_LoadingScreenScene, LoadSceneMode.Additive);
        Scene loadingScene = SceneManager.GetSceneByName(k_LoadingScreenScene);

        // Step 2: Begin loading the target scene without activating it yet
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        loadOp.allowSceneActivation = false;

        // Step 3: Show progress on the loading screen
        while (loadOp.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(loadOp.progress / 0.9f);
            m_LoadingScreen.SetProgress(progress);
            await Awaitable.NextFrameAsync();
        }

        // Step 4: Minimum display time to avoid loading screen flash
        m_LoadingScreen.SetProgress(1f);
        await Awaitable.WaitForSecondsAsync(0.5f);

        // Step 5: Activate the target scene
        loadOp.allowSceneActivation = true;
        while (!loadOp.isDone)
            await Awaitable.NextFrameAsync();

        // Step 6: Set the new scene as active
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(targetSceneName));

        // Step 7: Unload the old scene (if there is one to unload) and the loading screen
        // (caller is responsible for passing the previous scene name if needed)
        await SceneManager.UnloadSceneAsync(loadingScene);

        Debug.Log($"[Transition] Scene ready: {targetSceneName}");
    }
}

// Simple loading screen controller
public class LoadingScreenController : MonoBehaviour
{
    [SerializeField] private Slider m_ProgressBar;
    [SerializeField] private TMP_Text m_PercentageText;

    public void SetProgress(float normalizedProgress)
    {
        m_ProgressBar.value = normalizedProgress;
        m_PercentageText.text = $"{normalizedProgress:P0}";
    }
}
```
