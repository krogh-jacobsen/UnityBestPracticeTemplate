
# GitHub Copilot Instructions: Unity Addressables

Use this cheat sheet for LLM completions. See the readme.md for the general rationale behind these guidelines.

Table of contents:
- [Unity Version-Specific Instructions](#unity-version-specific-instructions)
- [Core Concepts](#core-concepts)
- [Loading Assets](#loading-assets)
- [Instantiating Addressables](#instantiating-addressables)
- [Releasing Handles — The Most Critical Rule](#releasing-handles--the-most-critical-rule)
- [Common Memory Leak Pattern and the Fix](#common-memory-leak-pattern-and-the-fix)
- [Batch Loading with Labels](#batch-loading-with-labels)
- [Groups and Profiles](#groups-and-profiles)
- [Preloading and Downloading Dependencies](#preloading-and-downloading-dependencies)
- [IResourceLocator](#iresourcelocator)
- [Never Use Resources.Load](#never-use-resourcesload)

# Unity Version-Specific Instructions

- ℹ️ This project uses Unity 6.3 and Addressables 2.x. API surface is stable but prefer the async/await pattern using `AsyncOperationHandle.Task` over `WaitForCompletion()`.
- ℹ️ `Addressables.LoadAssetAsync<T>()` returns an `AsyncOperationHandle<T>` — always store the handle in order to release it later.
- ℹ️ Unity 6 allows `await handle.Task` directly, making Awaitable-style patterns cleaner than coroutines.
- ℹ️ `WaitForCompletion()` is synchronous and blocks the main thread — avoid it in production code.

# Core Concepts

- ✅ Addressables decouple asset loading from asset location — the same code works for local and remote assets.
- ✅ Every asset is identified by an **address** (string key), a **label**, or a direct `AssetReference`.
- ✅ Use `AssetReference` fields in the Inspector instead of string addresses where possible — they are type-safe.
- ❌ Do not use `Resources.Load` when Addressables is available in the project.
- ❌ Do not hardcode address strings scattered across scripts — centralise them in a static constants class or use `AssetReference`.

```csharp
// Centralised address constants — avoid scattered magic strings
public static class AddressKeys
{
    public const string PlayerPrefab = "Prefabs/Player";
    public const string MainMenuScene = "Scenes/MainMenu";
    public const string EnemyLabel = "enemies";
}
```

# Loading Assets

- ✅ Use `Addressables.LoadAssetAsync<T>(key)` to load a single asset.
- ✅ Store the returned `AsyncOperationHandle<T>` — you need it to release the asset later.
- ✅ Check `handle.Status == AsyncOperationStatus.Succeeded` before using the result.
- ❌ Never access `handle.Result` without checking the status or awaiting completion.

```csharp
public class WeaponLoader : MonoBehaviour
{
    [SerializeField] private AssetReferenceGameObject m_WeaponReference;

    private AsyncOperationHandle<GameObject> m_LoadHandle;

    private async void Start()
    {
        m_LoadHandle = Addressables.LoadAssetAsync<GameObject>(m_WeaponReference);
        await m_LoadHandle.Task;

        if (m_LoadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject weaponPrefab = m_LoadHandle.Result;
            Instantiate(weaponPrefab, transform);
        }
        else
        {
            Debug.LogError($"[WeaponLoader] Failed to load weapon: {m_LoadHandle.OperationException}");
        }
    }

    private void OnDestroy()
    {
        // Always release the handle when done
        if (m_LoadHandle.IsValid())
            Addressables.Release(m_LoadHandle);
    }
}
```

# Instantiating Addressables

- ✅ Use `Addressables.InstantiateAsync(key)` when you want Addressables to manage the asset lifecycle tied to the instance.
- ✅ Use `Addressables.LoadAssetAsync<GameObject>` + `Instantiate` when you want to instantiate many copies from one loaded asset (pool pattern).
- ✅ Release instances created with `InstantiateAsync` using `Addressables.ReleaseInstance(gameObject)` — **not** `Destroy`.
- ❌ Never call `Destroy` on a GameObject created with `InstantiateAsync` — it leaks the underlying asset.

```csharp
// Pattern A: Addressables manages the instance lifecycle
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private AssetReferenceGameObject m_EnemyReference;

    private readonly List<GameObject> m_SpawnedEnemies = new List<GameObject>();

    public async void SpawnEnemy(Vector3 position)
    {
        var handle = Addressables.InstantiateAsync(m_EnemyReference, position, Quaternion.identity);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
            m_SpawnedEnemies.Add(handle.Result);
    }

    private void OnDestroy()
    {
        foreach (var enemy in m_SpawnedEnemies)
        {
            if (enemy != null)
                Addressables.ReleaseInstance(enemy); // NOT Destroy(enemy)
        }
        m_SpawnedEnemies.Clear();
    }
}

// Pattern B: Load once, instantiate many (preferred for pooling)
public class BulletPool : MonoBehaviour
{
    [SerializeField] private AssetReferenceGameObject m_BulletReference;

    private AsyncOperationHandle<GameObject> m_BulletHandle;
    private GameObject m_BulletPrefab;

    private async void Awake()
    {
        m_BulletHandle = Addressables.LoadAssetAsync<GameObject>(m_BulletReference);
        m_BulletPrefab = await m_BulletHandle.Task;
    }

    public GameObject SpawnBullet(Vector3 position)
    {
        return Instantiate(m_BulletPrefab, position, Quaternion.identity);
    }

    private void OnDestroy()
    {
        if (m_BulletHandle.IsValid())
            Addressables.Release(m_BulletHandle);
    }
}
```

# Releasing Handles — The Most Critical Rule

- ✅ Every `AsyncOperationHandle` **must** be released exactly once via `Addressables.Release(handle)`.
- ✅ Check `handle.IsValid()` before releasing to avoid double-release exceptions.
- ❌ Never release a handle more than once — it triggers an `InvalidOperationException`.
- ❌ Never lose the handle reference without releasing it — this is the most common Addressables memory leak.

```csharp
// Safe release helper
private void ReleaseHandle<T>(ref AsyncOperationHandle<T> handle)
{
    if (handle.IsValid())
    {
        Addressables.Release(handle);
        handle = default;
    }
}
```

# Common Memory Leak Pattern and the Fix

```csharp
// LEAK — handle is not stored, asset is never released
private async void LoadAndForget()
{
    var go = await Addressables.LoadAssetAsync<GameObject>("Prefabs/Player").Task;
    Instantiate(go);
    // handle is lost — the asset stays in memory forever
}

// CORRECT — store the handle and release it on cleanup
public class PlayerSpawner : MonoBehaviour
{
    private AsyncOperationHandle<GameObject> m_PlayerHandle;

    private async void Start()
    {
        m_PlayerHandle = Addressables.LoadAssetAsync<GameObject>("Prefabs/Player");
        await m_PlayerHandle.Task;

        if (m_PlayerHandle.Status == AsyncOperationStatus.Succeeded)
            Instantiate(m_PlayerHandle.Result);
    }

    private void OnDestroy()
    {
        if (m_PlayerHandle.IsValid())
            Addressables.Release(m_PlayerHandle);
    }
}
```

# Batch Loading with Labels

- ✅ Use `Addressables.LoadAssetsAsync<T>(label, callback)` to load all assets sharing a label.
- ✅ Use a single handle for the entire batch — release it once to unload all assets in the batch.
- ✅ Labels are assigned in the **Addressables Groups** window — no code changes needed to add assets to a batch.

```csharp
public class IconLibrary : MonoBehaviour
{
    private AsyncOperationHandle<IList<Sprite>> m_IconsHandle;
    private readonly Dictionary<string, Sprite> m_Icons = new Dictionary<string, Sprite>();

    private async void Awake()
    {
        m_IconsHandle = Addressables.LoadAssetsAsync<Sprite>("ui-icons", OnIconLoaded);
        await m_IconsHandle.Task;

        Debug.Log($"[IconLibrary] Loaded {m_Icons.Count} icons.");
    }

    private void OnIconLoaded(Sprite sprite)
    {
        m_Icons[sprite.name] = sprite;
    }

    public Sprite GetIcon(string iconName)
    {
        return m_Icons.TryGetValue(iconName, out var sprite) ? sprite : null;
    }

    private void OnDestroy()
    {
        if (m_IconsHandle.IsValid())
            Addressables.Release(m_IconsHandle);
    }
}
```

# Groups and Profiles

- ℹ️ **Groups** define how assets are bundled and where they are stored (Local vs Remote).
- ℹ️ **Local** groups are included in the build — no download required at runtime.
- ℹ️ **Remote** groups are hosted on a CDN — downloaded at runtime via `DownloadDependenciesAsync`.
- ✅ Put frequently updated assets (DLC, events) in Remote groups and stable assets in Local groups.
- ✅ Use **Profiles** to switch between development (local server) and production (CDN) without code changes.
- ❌ Do not put large textures or audio in Local groups unless they are always required at launch.

# Preloading and Downloading Dependencies

- ✅ Use `Addressables.GetDownloadSizeAsync(key)` to check whether a download is needed before starting.
- ✅ Use `Addressables.DownloadDependenciesAsync(key)` to preload remote bundles before loading assets.
- ✅ Show a progress bar using `handle.GetDownloadStatus().Percent` while downloading.
- ❌ Never call `LoadAssetAsync` on a remote asset without first ensuring the bundle is downloaded — it will fail silently or with a cryptic error.

```csharp
public class ContentDownloader : MonoBehaviour
{
    public async void DownloadLevel(string levelLabel, Action onComplete)
    {
        var sizeHandle = Addressables.GetDownloadSizeAsync(levelLabel);
        await sizeHandle.Task;
        long downloadSize = sizeHandle.Result;
        Addressables.Release(sizeHandle);

        if (downloadSize == 0)
        {
            onComplete?.Invoke();
            return;
        }

        var downloadHandle = Addressables.DownloadDependenciesAsync(levelLabel);
        while (!downloadHandle.IsDone)
        {
            float progress = downloadHandle.GetDownloadStatus().Percent;
            Debug.Log($"[Download] {progress:P0}");
            await Awaitable.NextFrameAsync();
        }

        Addressables.Release(downloadHandle);
        onComplete?.Invoke();
    }
}
```

# IResourceLocator

- ℹ️ `IResourceLocator` maps keys (addresses, labels) to `IResourceLocation` lists — rarely needed directly.
- ✅ Use `Addressables.LoadResourceLocationsAsync(key)` if you need to check whether an address exists before loading.
- ❌ Do not iterate `Addressables.ResourceLocators` directly in production code — it couples you to the catalog structure.

```csharp
// Check if an address exists at runtime
private async Task<bool> AddressExistsAsync(string address)
{
    var handle = Addressables.LoadResourceLocationsAsync(address);
    await handle.Task;
    bool exists = handle.Status == AsyncOperationStatus.Succeeded && handle.Result.Count > 0;
    Addressables.Release(handle);
    return exists;
}
```

# Never Use Resources.Load

- ❌ Do not use `Resources.Load<T>()` in any new code when Addressables is set up for the project.
- ❌ Do not store assets in a `Resources/` folder — everything in `Resources/` is included in the build regardless of whether it is used.
- ✅ If migrating from Resources to Addressables, move assets out of `Resources/` and mark them as Addressable in the Groups window.
- ✅ The `ResourcesChecker` tool in the Package Manager can help identify remaining Resources references.

```csharp
// AVOID
var prefab = Resources.Load<GameObject>("Prefabs/Enemy"); // includes in build, no async, no remote support

// PREFER
var handle = Addressables.LoadAssetAsync<GameObject>("Prefabs/Enemy");
await handle.Task;
var prefab = handle.Result;
```
