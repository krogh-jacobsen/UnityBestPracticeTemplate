
# GitHub Copilot Instructions: Unity Netcode for GameObjects

Use this cheat sheet for LLM completions. See the readme.md for the general rationale behind these guidelines.

Table of contents:
- [Unity Version-Specific Instructions](#unity-version-specific-instructions)
- [Core Concepts](#core-concepts)
- [NetworkManager](#networkmanager)
- [NetworkObject](#networkobject)
- [NetworkBehaviour](#networkbehaviour)
- [NetworkVariable](#networkvariable)
- [ServerRpc and ClientRpc](#serverrpc-and-clientrpc)
- [Ownership](#ownership)
- [Spawning and Despawning](#spawning-and-despawning)
- [NetworkTransform and NetworkAnimator](#networktransform-and-networkanimator)
- [Common Mistakes](#common-mistakes)
- [Ownership Check Pattern](#ownership-check-pattern)
- [Client-Side Prediction Basics](#client-side-prediction-basics)

# Unity Version-Specific Instructions

- ℹ️ This project uses Unity 6.3 and **Netcode for GameObjects (NGO) 2.x**. NGO 2.x introduced breaking changes from 1.x — do not reference NGO 1.x documentation.
- ℹ️ NGO 2.x requires Unity 6.0 or later. The package ID is `com.unity.netcode.gameobjects`.
- ℹ️ NGO 2.x adds `NetworkVariable` write permissions per-client (not just server-authoritative) — always declare explicit `NetworkVariableWritePermission` and `NetworkVariableReadPermission`.
- ℹ️ Unity 6 + NGO 2.x supports `Awaitable`-based connection flows — prefer async patterns over coroutines for connection setup.

# Core Concepts

- ✅ NGO is **server-authoritative** — the server owns game state; clients request changes via `ServerRpc`.
- ✅ The server validates all `ServerRpc` inputs before applying them to `NetworkVariable` state.
- ✅ All networked objects must have a `NetworkObject` component on the **root** `GameObject`.
- ✅ All networked behaviour scripts must inherit from `NetworkBehaviour`, not `MonoBehaviour`.
- ❌ Never modify `NetworkVariable` values from the client — only the server (or an object owner if permissions allow) may write.
- ❌ Never put `NetworkObject` on a child `GameObject` — it must always be on the root.

# NetworkManager

- ✅ Place one `NetworkManager` in the scene (persistent scene or bootstrap scene) — it must not be destroyed across scene loads.
- ✅ Configure the `NetworkManager` with a transport (e.g. `UnityTransport`) and a `NetworkPrefabsList` in the Inspector.
- ✅ Start the host (`NetworkManager.Singleton.StartHost()`) for listen-server or offline play; use `StartServer()` + `StartClient()` for dedicated server architecture.
- ❌ Never call `StartHost`/`StartServer`/`StartClient` more than once per session without first stopping the previous session.

```csharp
public class NetworkBootstrapper : MonoBehaviour
{
    public async Awaitable StartAsHost()
    {
        bool started = NetworkManager.Singleton.StartHost();
        if (!started)
        {
            Debug.LogError("[Network] Failed to start as host.");
            return;
        }

        Debug.Log("[Network] Host started. Waiting for clients...");
    }

    public async Awaitable StartAsClient(string ipAddress, ushort port)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ipAddress, port);

        bool started = NetworkManager.Singleton.StartClient();
        if (!started)
            Debug.LogError("[Network] Failed to start client connection.");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();
    }
}
```

# NetworkObject

- ✅ Add `NetworkObject` to the **root** `GameObject` of every prefab that needs to exist across the network.
- ✅ Register networked prefabs in the `NetworkManager`'s `NetworkPrefabsList` — unregistered prefabs cannot be spawned via NGO.
- ✅ Use `NetworkObject.NetworkObjectId` to uniquely identify objects across the network.
- ❌ Never instantiate networked prefabs with `Instantiate` and then call `Spawn` — always use `NetworkObject.Spawn` on the server after the object is instantiated.
- ❌ Never add `NetworkObject` to a child transform — it must be on the root.

# NetworkBehaviour

- ✅ Inherit from `NetworkBehaviour` instead of `MonoBehaviour` for any script that uses NGO features.
- ✅ Use `OnNetworkSpawn()` instead of `Start()` or `Awake()` for initialisation that requires the network to be ready.
- ✅ Use `OnNetworkDespawn()` instead of `OnDestroy()` for network-related cleanup.
- ✅ Check `IsServer`, `IsClient`, `IsHost`, `IsOwner` before executing code that should only run on specific sides.
- ❌ Never use `Awake`/`Start` for logic that depends on `IsServer`/`IsClient` — the network connection is not established yet.

```csharp
public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private int m_MaxHealth = 100;

    private NetworkVariable<int> m_CurrentHealth =
        new NetworkVariable<int>(
            value: 100,
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Subscribe to value changes on all clients
        m_CurrentHealth.OnValueChanged += HandleHealthChanged;

        if (IsServer)
            m_CurrentHealth.Value = m_MaxHealth;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        m_CurrentHealth.OnValueChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int previousValue, int newValue)
    {
        Debug.Log($"[Health] Changed from {previousValue} to {newValue}");
        // Update UI, play VFX, etc. — runs on all clients
    }
}
```

# NetworkVariable

- ✅ Use `NetworkVariable<T>` for authoritative state that all clients need: health, score, position, game phase.
- ✅ Always declare explicit `readPerm` and `writePerm` — do not rely on defaults which may change between NGO versions.
- ✅ Subscribe to `NetworkVariable<T>.OnValueChanged` in `OnNetworkSpawn` and unsubscribe in `OnNetworkDespawn`.
- ✅ Supported types: `int`, `float`, `bool`, `Vector3`, `Quaternion`, structs marked with `INetworkSerializable`, and `FixedString` types.
- ❌ Never write to a `NetworkVariable` from the client side unless `writePerm = NetworkVariableWritePermission.Owner` and the client is the owner.
- ❌ Never use `NetworkVariable<string>` — use `FixedString64Bytes` or `FixedString128Bytes` from `Unity.Collections`.

```csharp
// Correct NetworkVariable declarations
private NetworkVariable<int> m_Score = new NetworkVariable<int>(
    0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

private NetworkVariable<float> m_Health = new NetworkVariable<float>(
    100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

private NetworkVariable<Vector3> m_TargetPosition = new NetworkVariable<Vector3>(
    Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

// Avoid
private NetworkVariable<string> m_PlayerName; // string is not supported
```

# ServerRpc and ClientRpc

- ✅ `[ServerRpc]` — client calls this method, it executes on the server. Method name must end in `ServerRpc`.
- ✅ `[ClientRpc]` — server calls this method, it executes on all clients. Method name must end in `ClientRpc`.
- ✅ Add `[ServerRpc(RequireOwnership = true)]` (default) to restrict to the owning client. Use `RequireOwnership = false` for RPCs callable by any client.
- ✅ Add `[ServerRpc(RequireOwnership = false)]` when any client (not just the owner) must be able to invoke the RPC.
- ❌ Never call a `[ClientRpc]` method from a client — it must be called from the server.
- ❌ Never call a `[ServerRpc]` method from the server — it must be called from a client.
- ❌ Never pass reference types (classes, arrays) directly in RPC parameters without implementing `INetworkSerializable`.

```csharp
public class PlayerAttack : NetworkBehaviour
{
    // Client calls this — executes on server
    [ServerRpc]
    public void FireWeaponServerRpc(Vector3 origin, Vector3 direction)
    {
        // Server validates and processes the attack
        if (!IsServer) return; // defensive check — redundant but explicit

        bool hitEnemy = Physics.Raycast(origin, direction, out RaycastHit hit, 100f);
        if (hitEnemy && hit.collider.TryGetComponent<EnemyHealth>(out var health))
        {
            health.TakeDamage(10);
            NotifyHitClientRpc(hit.point);
        }
    }

    // Server calls this — executes on all clients
    [ClientRpc]
    private void NotifyHitClientRpc(Vector3 hitPoint)
    {
        // Spawn hit VFX on all clients
        Debug.Log($"[Network] Hit at {hitPoint}");
    }
}
```

# Ownership

- ✅ Use `IsOwner` to restrict input and local prediction logic to the owning client.
- ✅ Use `IsServer` to restrict authoritative game logic to the server.
- ✅ Use `IsClient` to restrict client-only visual/audio feedback.
- ✅ Use `OwnerClientId` to identify which client owns an object.
- ❌ Never apply player input on `!IsOwner` clients — each player controls only their own character.

```csharp
public class PlayerController : NetworkBehaviour
{
    private void Update()
    {
        // Only the owner processes input
        if (!IsOwner) return;

        Vector2 moveInput = m_CachedMove; // from Input System callbacks
        MovePlayerServerRpc(moveInput);
    }

    [ServerRpc]
    private void MovePlayerServerRpc(Vector2 input)
    {
        // Server applies movement — position replicated to clients via NetworkTransform
        Vector3 velocity = new Vector3(input.x, 0f, input.y) * 5f;
        transform.position += velocity * Time.deltaTime;
    }
}
```

# Spawning and Despawning

- ✅ Only the server can spawn and despawn `NetworkObject` instances.
- ✅ After `Instantiate`, call `networkObject.Spawn()` to register the object with NGO and replicate it to all clients.
- ✅ Use `networkObject.SpawnWithOwnership(clientId)` to give a client ownership at spawn time.
- ✅ Use `networkObject.SpawnAsPlayerObject(clientId)` for player-controlled objects.
- ✅ Call `networkObject.Despawn()` (not `Destroy`) to remove the object across all clients.
- ❌ Never call `Destroy` on a spawned `NetworkObject` from the server — use `Despawn()` followed by optional `Destroy`.
- ❌ Never spawn objects from a client — only call `SpawnServerRpc` patterns from client, let server execute the actual spawn.

```csharp
// Server-side spawner
public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] private GameObject m_EnemyPrefab;

    public void SpawnEnemy(Vector3 position)
    {
        if (!IsServer)
        {
            Debug.LogWarning("[Spawn] SpawnEnemy must be called from the server.");
            return;
        }

        GameObject instance = Instantiate(m_EnemyPrefab, position, Quaternion.identity);
        NetworkObject netObj = instance.GetComponent<NetworkObject>();
        netObj.Spawn(); // Replicates to all connected clients
    }

    public void DespawnEnemy(NetworkObject enemy)
    {
        if (!IsServer) return;

        enemy.Despawn(); // Removes from all clients
        Destroy(enemy.gameObject); // Destroy locally after despawn
    }
}
```

# NetworkTransform and NetworkAnimator

- ✅ Add `NetworkTransform` to synchronise `Transform` (position, rotation, scale) across the network.
- ✅ Disable axes you do not need in `NetworkTransform` — synchronising unused axes wastes bandwidth.
- ✅ Set `NetworkTransform.AuthorityMode` to `Server` for server-authoritative movement and `Owner` for client-authoritative (with lag compensation).
- ✅ Add `NetworkAnimator` to synchronise `Animator` state across the network.
- ❌ Never manually synchronise `transform.position` via `NetworkVariable<Vector3>` when `NetworkTransform` is present — they will conflict.
- ❌ Do not use `NetworkAnimator` for parameters set every frame — it generates heavy traffic. Drive animations from `NetworkVariable` state changes instead when possible.

# Common Mistakes

| Mistake | Consequence | Fix |
|---|---|---|
| Calling `[ClientRpc]` from a client | Throws an exception; only server can invoke `ClientRpc` | Add `if (!IsServer) return;` guard |
| Calling `[ServerRpc]` from the server | Has no effect; only clients invoke `ServerRpc` | Add `if (!IsClient) return;` guard |
| Writing to `NetworkVariable` from a client | Logs an error; value is not changed | Validate server ownership before writing |
| `NetworkObject` on a child `GameObject` | NGO throws an error at spawn | Move `NetworkObject` to root |
| Using `Destroy` instead of `Despawn` | Object is destroyed locally but not on clients (or vice versa) | Always call `networkObject.Despawn()` |
| Forgetting to register prefabs | Runtime spawn fails with "prefab not registered" error | Add all networked prefabs to `NetworkPrefabsList` |
| Logic in `Start()` that checks `IsServer` | Network not ready yet — `IsServer` may return incorrect value | Move to `OnNetworkSpawn()` |

# Ownership Check Pattern

Use this pattern consistently at the top of methods that should only execute on a specific network role:

```csharp
public class NetworkedItem : NetworkBehaviour
{
    // Runs on all instances — guard by role
    private void Update()
    {
        if (!IsOwner) return; // Only the owner updates input
        ProcessLocalInput();
    }

    private void ProcessLocalInput()
    {
        // Safe — only runs on the owning client
    }

    [ServerRpc]
    private void RequestPickupServerRpc()
    {
        // Called by client — executes on server
        // IsServer is true here by definition, but defensive check is fine for clarity:
        if (!IsServer) return;

        GrantPickupClientRpc();
    }

    [ClientRpc]
    private void GrantPickupClientRpc()
    {
        // Runs on all clients — trigger pickup VFX
        Debug.Log("[Item] Pickup confirmed.");
    }
}
```

# Client-Side Prediction Basics

- ✅ Apply owner input locally on the client immediately — do not wait for server round-trip for responsive feel.
- ✅ Send the same input to the server via `ServerRpc` — the server applies it authoritatively.
- ✅ If the server result differs from the client prediction, reconcile by snapping or smoothing to the server state.
- ❌ Full client-side prediction is complex — use NGO's built-in `NetworkTransform` with owner authority as the simplest starting point.

```csharp
public class PredictedPlayerMovement : NetworkBehaviour
{
    [SerializeField] private float m_Speed = 5f;

    private Vector2 m_Input;

    private void Update()
    {
        if (!IsOwner) return;

        // Apply locally for instant response
        Vector3 localMove = new Vector3(m_Input.x, 0f, m_Input.y) * m_Speed * Time.deltaTime;
        transform.position += localMove;

        // Send to server for authoritative processing
        if (m_Input != Vector2.zero)
            SendInputServerRpc(m_Input);
    }

    [ServerRpc]
    private void SendInputServerRpc(Vector2 input)
    {
        // Server applies the same movement
        // NetworkTransform will correct any divergence on the client
        Vector3 serverMove = new Vector3(input.x, 0f, input.y) * m_Speed * Time.deltaTime;
        transform.position += serverMove;
    }
}
```
