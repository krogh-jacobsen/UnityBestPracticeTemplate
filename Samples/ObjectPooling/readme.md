# Sample: Object Pooling

Demonstrates Unity's built-in `ObjectPool<T>` for efficiently reusing GameObjects without repeated Instantiate/Destroy calls.

## How it works

- `PooledSpawner` owns and manages an `ObjectPool<PooledProjectile>`
- On spawn, it calls `m_Pool.Get()` which activates a pooled instance (or creates one if the pool is empty)
- `PooledProjectile` calls `m_Spawner.ReturnToPool(this)` after its lifetime, which calls `m_Pool.Release()` and deactivates it
- The pool reuses instances, avoiding GC pressure from Instantiate/Destroy

## Setup

1. Create a prefab with `PooledProjectile` attached
2. Add `PooledSpawner` to a GameObject in your scene
3. Assign the prefab to **Projectile Prefab**
4. Press Play — projectiles spawn, move, and return to the pool automatically

## Key takeaways

- Never `Destroy` pooled objects — call `Release()` instead
- `actionOnGet` / `actionOnRelease` toggle `SetActive` — don't use the constructor for init
- Use `Awaitable.WaitForSecondsAsync` (Unity 6+) instead of coroutines for simple delays
- Guard continuations: `if (this == null || m_Spawner == null) return;` after any await
