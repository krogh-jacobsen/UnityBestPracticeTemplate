
# GitHub Copilot Instructions: Unity Animation

Use this cheat sheet for LLM completions. See the readme.md for the general rationale behind these guidelines.

Table of contents:
- [Unity Version-Specific Instructions](#unity-version-specific-instructions)
- [Animator Parameter IDs — Cache Everything](#animator-parameter-ids--cache-everything)
- [Parameter Hash Constants Class](#parameter-hash-constants-class)
- [Setting Parameters Correctly](#setting-parameters-correctly)
- [Blend Trees](#blend-trees)
- [AnimatorOverrideController](#animatoroverridecontroller)
- [Animation Events](#animation-events)
- [CrossFade and Transitions](#crossfade-and-transitions)
- [Avatar Masks](#avatar-masks)
- [Root Motion](#root-motion)
- [RuntimeAnimatorController](#runtimeanimatorcontroller)

# Unity Version-Specific Instructions

- ℹ️ This project uses Unity 6.3. The `Animator` API is unchanged from Unity 2022, but Unity 6 introduces improvements to the Animation Rigging package.
- ℹ️ Unity 6 deprecates some legacy `AnimationClip.SetCurve` patterns in favour of the new Animation Authoring workflow — prefer timeline-based clip authoring.
- ℹ️ `Animator.StringToHash()` has been the recommended approach since Unity 5 — no exceptions for Unity 6.
- ℹ️ Unity 6 supports a new `AnimatorControllerLayer.syncedLayerAffectsTiming` property — useful for synced layers.

# Animator Parameter IDs — Cache Everything

- ✅ Always convert parameter names to integer hashes using `Animator.StringToHash()`.
- ✅ Store hashes as `static readonly int` fields — compute them once, reuse everywhere.
- ❌ Never pass string literals to `SetBool`, `SetFloat`, `SetInteger`, or `SetTrigger` in hot paths.
- ❌ Never call `Animator.StringToHash()` inside `Update`, `FixedUpdate`, or any per-frame method.

The string overloads of the `Set*` methods perform a hash lookup on every call. Using pre-computed integer hashes eliminates this overhead entirely.

```csharp
// AVOID — string lookup every frame
private void Update()
{
    m_Animator.SetFloat("Speed", m_Speed);   // hashes "Speed" each call
    m_Animator.SetBool("IsGrounded", m_IsGrounded);
}

// PREFER — hash computed once at class initialisation
private static readonly int k_SpeedHash = Animator.StringToHash("Speed");
private static readonly int k_IsGroundedHash = Animator.StringToHash("IsGrounded");

private void Update()
{
    m_Animator.SetFloat(k_SpeedHash, m_Speed);
    m_Animator.SetBool(k_IsGroundedHash, m_IsGrounded);
}
```

# Parameter Hash Constants Class

For projects with many animators, centralise all hashes in a dedicated static class. This prevents duplication and makes refactoring safer.

```csharp
/// <summary>
/// Cached Animator parameter hashes. Use these integer overloads in all
/// Animator.Set* calls to avoid per-frame string hashing.
/// </summary>
public static class AnimatorParams
{
    // Locomotion
    public static readonly int Speed = Animator.StringToHash("Speed");
    public static readonly int VerticalSpeed = Animator.StringToHash("VerticalSpeed");
    public static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    public static readonly int IsCrouching = Animator.StringToHash("IsCrouching");

    // Combat
    public static readonly int AttackTrigger = Animator.StringToHash("Attack");
    public static readonly int HitTrigger = Animator.StringToHash("Hit");
    public static readonly int DieTrigger = Animator.StringToHash("Die");

    // State layer indices
    public static readonly int BaseLayerIndex = 0;
    public static readonly int UpperBodyLayerIndex = 1;
}

// Usage in MonoBehaviour
public class CharacterAnimator : MonoBehaviour
{
    private Animator m_Animator;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    public void SetSpeed(float speed)
    {
        m_Animator.SetFloat(AnimatorParams.Speed, speed);
    }

    public void TriggerAttack()
    {
        m_Animator.SetTrigger(AnimatorParams.AttackTrigger);
    }

    public void SetGrounded(bool grounded)
    {
        m_Animator.SetBool(AnimatorParams.IsGrounded, grounded);
    }
}
```

# Setting Parameters Correctly

- ✅ `SetFloat(hash, value)` — for blend tree parameters and continuous values.
- ✅ `SetBool(hash, value)` — for state toggles (IsGrounded, IsCrouching, IsAlive).
- ✅ `SetInteger(hash, value)` — for discrete state selectors (weapon type index, combo counter).
- ✅ `SetTrigger(hash)` — for one-shot events (attack, jump, hit reaction).
- ✅ `ResetTrigger(hash)` — call when cancelling an action to prevent stale triggers firing later.
- ❌ Never use `SetTrigger` for continuous state — a trigger fires once and resets; use `SetBool` instead.
- ❌ Never forget to call `ResetTrigger` when an action is interrupted — orphaned triggers cause unexpected state transitions.

```csharp
// Correctly resetting a trigger on action cancel
public void CancelAttack()
{
    m_Animator.ResetTrigger(AnimatorParams.AttackTrigger);
    m_IsAttacking = false;
}
```

# Blend Trees

- ✅ Use 1D blend trees for directional locomotion driven by a single float (Speed).
- ✅ Use 2D Freeform Directional blend trees for strafing locomotion driven by two floats (VelocityX, VelocityZ).
- ✅ Smooth blend tree inputs over time using `Mathf.Lerp` or `Mathf.MoveTowards` before passing to `SetFloat`.
- ❌ Never set blend tree parameters to raw, un-smoothed values — this causes jittery animation blending.

```csharp
private static readonly int k_SpeedXHash = Animator.StringToHash("VelocityX");
private static readonly int k_SpeedZHash = Animator.StringToHash("VelocityZ");

[SerializeField] private float m_BlendSpeed = 5f;

private Vector2 m_CurrentBlend;

private void Update()
{
    Vector2 targetBlend = new Vector2(m_Velocity.x, m_Velocity.z);
    m_CurrentBlend = Vector2.MoveTowards(m_CurrentBlend, targetBlend, m_BlendSpeed * Time.deltaTime);

    m_Animator.SetFloat(k_SpeedXHash, m_CurrentBlend.x);
    m_Animator.SetFloat(k_SpeedZHash, m_CurrentBlend.y);
}
```

# AnimatorOverrideController

- ✅ Use `AnimatorOverrideController` to swap individual animation clips at runtime without duplicating the controller.
- ✅ Apply overrides in bulk using `AnimatorOverrideController.ApplyOverrides(list)` to avoid triggering a rebuild per clip.
- ❌ Never assign clips one at a time in a loop using the indexer (`controller["ClipName"] = newClip`) — each assignment triggers an animator rebuild.

```csharp
public class WeaponAnimationSwapper : MonoBehaviour
{
    private Animator m_Animator;
    private AnimatorOverrideController m_OverrideController;

    private readonly List<KeyValuePair<AnimationClip, AnimationClip>> m_Overrides =
        new List<KeyValuePair<AnimationClip, AnimationClip>>();

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_OverrideController = new AnimatorOverrideController(m_Animator.runtimeAnimatorController);
        m_Animator.runtimeAnimatorController = m_OverrideController;
    }

    public void ApplyWeaponAnimations(AnimationClip attackClip, AnimationClip idleClip)
    {
        m_Overrides.Clear();
        // Map original clips (keys) to replacement clips (values)
        m_Overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(
            m_OverrideController["BaseAttack"], attackClip));
        m_Overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(
            m_OverrideController["BaseIdle"], idleClip));

        // Apply all overrides in a single pass — one animator rebuild
        m_OverrideController.ApplyOverrides(m_Overrides);
    }
}
```

# Animation Events

- ✅ Animation events call public or private methods on `MonoBehaviour` components on the same `GameObject` as the `Animator`.
- ✅ Declare animation event methods as `private void` — Unity's animation system finds them via reflection; they do not need to be public.
- ✅ Use animation events for gameplay-critical frame-synchronised effects: footstep sounds, VFX spawns, hitbox activation.
- ❌ Never use animation events for logic that must not miss a frame — if the animator is disabled, events are skipped.
- ❌ Do not pass complex data through animation event parameters — they support only `int`, `float`, `string`, and `Object` references.

```csharp
public class CharacterFX : MonoBehaviour
{
    [SerializeField] private ParticleSystem m_FootstepFX;
    [SerializeField] private AudioClip m_FootstepClip;
    [SerializeField] private AudioSource m_AudioSource;

    // Called from Animation Event on the walk/run clip at each footfall frame
    private void OnFootstep()
    {
        m_FootstepFX.Play();
        m_AudioSource.PlayOneShot(m_FootstepClip);
    }

    // Called from Animation Event on the attack clip at the impact frame
    private void OnAttackImpact()
    {
        // Activate hitbox, trigger screen shake, etc.
    }
}
```

# CrossFade and Transitions

- ✅ Use `Animator.CrossFade(stateHash, normalizedTransitionDuration)` to trigger transitions programmatically with a fixed blend duration.
- ✅ Use `Animator.CrossFadeInFixedTime(stateHash, fixedDuration)` when you need the blend time in seconds rather than normalised time.
- ✅ Cache state name hashes with `Animator.StringToHash("StateName")` — same as parameter hashes.
- ❌ Avoid relying exclusively on Animator conditions for complex branching logic — drive transitions from code using `SetBool`/`SetTrigger` for clarity.

```csharp
private static readonly int k_DeathStateHash = Animator.StringToHash("Death");

public void PlayDeath()
{
    // Cross-fade to Death state over 0.2 seconds (normalised)
    m_Animator.CrossFade(k_DeathStateHash, normalizedTransitionDuration: 0.2f);
}
```

# Avatar Masks

- ✅ Use Avatar Masks to isolate animation to specific body regions (e.g. upper body only for aiming while lower body runs).
- ✅ Apply masks at the **layer** level in the Animator Controller — do not apply them in code.
- ✅ Set the layer weight and mask in `Awake` or `Start` via `Animator.SetLayerWeight(index, weight)` if transitioning dynamically.
- ❌ Avoid modifying layer weights every frame — cache the target weight and lerp to it.

```csharp
private static readonly int k_UpperBodyLayerIndex = 1;

[SerializeField] private float m_AimLayerBlendSpeed = 5f;
private float m_TargetAimWeight;

public void SetAiming(bool isAiming)
{
    m_TargetAimWeight = isAiming ? 1f : 0f;
}

private void Update()
{
    float current = m_Animator.GetLayerWeight(k_UpperBodyLayerIndex);
    float next = Mathf.MoveTowards(current, m_TargetAimWeight, m_AimLayerBlendSpeed * Time.deltaTime);
    m_Animator.SetLayerWeight(k_UpperBodyLayerIndex, next);
}
```

# Root Motion

- ✅ Enable `Animator.applyRootMotion = true` when the animation clip drives character position/rotation (e.g. melee attacks, cinematic movements).
- ✅ Override `OnAnimatorMove()` to intercept root motion and apply it through the `CharacterController` or `Rigidbody` for physics-correct movement.
- ❌ Never leave `applyRootMotion` enabled on a character driven by a `Rigidbody` without overriding `OnAnimatorMove` — it will fight physics.

```csharp
public class RootMotionDriver : MonoBehaviour
{
    private Animator m_Animator;
    private CharacterController m_Controller;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_Controller = GetComponent<CharacterController>();
        m_Animator.applyRootMotion = true;
    }

    // Called by Unity instead of applying root motion automatically
    private void OnAnimatorMove()
    {
        Vector3 rootDelta = m_Animator.deltaPosition;
        rootDelta.y = 0f; // Ignore vertical root motion — let gravity handle it
        m_Controller.Move(rootDelta);
    }
}
```

# RuntimeAnimatorController

- ✅ Swap the entire `Animator.runtimeAnimatorController` at runtime to switch a character's complete animation set (e.g. different character skins or weapon types with distinct full-body animations).
- ✅ Store `RuntimeAnimatorController` references as serialized fields — do not load them from Resources.
- ℹ️ Swapping `runtimeAnimatorController` resets all animator state — use `AnimatorOverrideController` instead if you only need to swap individual clips.

```csharp
public class CharacterAnimatorSwapper : MonoBehaviour
{
    [SerializeField] private RuntimeAnimatorController m_SwordController;
    [SerializeField] private RuntimeAnimatorController m_BowController;

    private Animator m_Animator;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    public void EquipSword()
    {
        m_Animator.runtimeAnimatorController = m_SwordController;
    }

    public void EquipBow()
    {
        m_Animator.runtimeAnimatorController = m_BowController;
    }
}
```
