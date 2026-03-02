
# GitHub Copilot Instructions: Unity Testing

Use this cheat sheet for LLM completions. See the readme.md for the general rationale behind these guidelines.

Table of contents:
- [Unity Version-Specific Instructions](#unity-version-specific-instructions)
- [Test Framework Overview](#test-framework-overview)
- [EditMode vs PlayMode Tests](#editmode-vs-playmode-tests)
- [Test Assembly Definitions](#test-assembly-definitions)
- [NUnit Attributes](#nunit-attributes)
- [Arrange-Act-Assert Pattern](#arrange-act-assert-pattern)
- [PlayMode Coroutine Tests](#playmode-coroutine-tests)
- [Error and Log Assertion](#error-and-log-assertion)
- [Mocking with Interfaces](#mocking-with-interfaces)
- [Naming Conventions](#naming-conventions)
- [Testing a State Machine](#testing-a-state-machine)
- [Code Coverage](#code-coverage)

# Unity Version-Specific Instructions

- ℹ️ This project uses Unity 6.3. Use the Unity Test Framework (UTF) 2.x which ships with Unity 6.
- ℹ️ Unity 6 supports `Awaitable`-based tests in PlayMode via `[UnityTest]` with `yield return` or via async test methods with the `UnityTestAttribute`.
- ℹ️ Prefer `[UnityTest]` with `IEnumerator` for PlayMode tests that need to wait for frames or physics.
- ℹ️ Use `UnityEngine.TestTools.LogAssert` to assert expected log messages before they occur.

# Test Framework Overview

- ℹ️ Unity Test Framework (UTF) is built on NUnit 3 and integrated directly into the Unity Editor.
- ✅ Access tests via **Window > General > Test Runner** in the Unity Editor.
- ✅ Tests run in two modes: EditMode (no play state required) and PlayMode (enters play state).
- ❌ Never mix EditMode and PlayMode tests in the same assembly — they require separate `.asmdef` files.

# EditMode vs PlayMode Tests

## When to use EditMode

- ✅ Use EditMode for pure logic: calculations, data transformations, ScriptableObject logic, utility classes.
- ✅ Use EditMode for tests that do not require a running scene or physics simulation.
- ✅ Use EditMode for testing editor-only tools and custom inspectors.
- ✅ EditMode tests run without entering play mode — they are fast and do not trigger `Awake`/`Start`.

```csharp
// EditMode test — no MonoBehaviour lifecycle, runs instantly
[TestFixture]
public class DamageCalculatorTests
{
    [Test]
    public void CalculateDamage_WithArmor_ReturnsReducedValue()
    {
        // Arrange
        var calculator = new DamageCalculator();

        // Act
        float result = calculator.CalculateDamage(baseDamage: 100f, armor: 25f);

        // Assert
        Assert.AreEqual(75f, result, 0.001f);
    }
}
```

## When to use PlayMode

- ✅ Use PlayMode when you need `Awake`, `Start`, `Update`, or `FixedUpdate` to run.
- ✅ Use PlayMode for testing physics, coroutines, animation, and scene interactions.
- ✅ Use PlayMode when the feature under test requires a `MonoBehaviour` lifecycle.
- ❌ Avoid PlayMode tests for pure logic — they are slower and require entering play state.

```csharp
// PlayMode test — scene is entered, MonoBehaviour lifecycle runs
[TestFixture]
public class PlayerControllerTests
{
    private GameObject m_PlayerGo;

    [SetUp]
    public void SetUp()
    {
        m_PlayerGo = new GameObject("Player");
        m_PlayerGo.AddComponent<PlayerController>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(m_PlayerGo);
    }

    [UnityTest]
    public IEnumerator PlayerController_AfterOneFrame_IsInitialized()
    {
        // Arrange — SetUp already ran

        // Act — wait one frame for Awake/Start to complete
        yield return null;

        // Assert
        var controller = m_PlayerGo.GetComponent<PlayerController>();
        Assert.IsTrue(controller.IsInitialized);
    }
}
```

# Test Assembly Definitions

- ✅ Always place tests in a dedicated assembly separate from production code.
- ✅ Name the test `.asmdef` after the assembly under test with a `.Tests` suffix: `MyGame.Gameplay.Tests`.
- ✅ Set `testPlatforms` to restrict the assembly to the correct test mode.
- ❌ Never reference test assemblies from production assemblies.
- ❌ Never add `NUnit` or `UnityEngine.TestRunner` references to production `.asmdef` files.

EditMode test assembly (`MyGame.Gameplay.EditModeTests.asmdef`):

```json
{
    "name": "MyGame.Gameplay.EditModeTests",
    "rootNamespace": "MyGame.Gameplay.Tests",
    "references": [
        "MyGame.Gameplay"
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

PlayMode test assembly (`MyGame.Gameplay.PlayModeTests.asmdef`):

```json
{
    "name": "MyGame.Gameplay.PlayModeTests",
    "rootNamespace": "MyGame.Gameplay.Tests",
    "references": [
        "MyGame.Gameplay"
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

# NUnit Attributes

- ✅ `[TestFixture]` — marks a class as containing tests (required for NUnit, optional in UTF but always include it for clarity).
- ✅ `[Test]` — marks a synchronous test method.
- ✅ `[UnityTest]` — marks an `IEnumerator` test that runs over multiple frames in PlayMode.
- ✅ `[SetUp]` — runs before each test method; use to create objects and reset state.
- ✅ `[TearDown]` — runs after each test method; use to destroy objects and release resources.
- ✅ `[OneTimeSetUp]` — runs once before all tests in the fixture; use for expensive shared setup.
- ✅ `[OneTimeTearDown]` — runs once after all tests in the fixture.
- ✅ `[Category("Performance")]` — tag tests by category to filter them in the Test Runner.
- ✅ `[TestCase(10f, 5f, 5f)]` — parameterized test cases to avoid duplicating test methods.

```csharp
[TestFixture]
[Category("Gameplay")]
public class HealthSystemTests
{
    private HealthSystem m_Health;

    [SetUp]
    public void SetUp()
    {
        m_Health = new HealthSystem(maxHealth: 100f);
    }

    [TearDown]
    public void TearDown()
    {
        m_Health = null;
    }

    [TestCase(10f, 90f)]
    [TestCase(100f, 0f)]
    [TestCase(0f, 100f)]
    public void TakeDamage_ReducesCurrentHealth(float damage, float expectedHealth)
    {
        m_Health.TakeDamage(damage);
        Assert.AreEqual(expectedHealth, m_Health.Current, 0.001f);
    }
}
```

# Arrange-Act-Assert Pattern

- ✅ Structure every test with three clearly separated sections: Arrange, Act, Assert.
- ✅ Use blank lines and comments (`// Arrange`, `// Act`, `// Assert`) to separate the sections visually.
- ✅ Each test should assert exactly one behaviour — split multi-behaviour tests into separate methods.
- ❌ Avoid logic inside the Assert section — compute expected values in the Arrange section.

```csharp
[Test]
public void Inventory_AddItem_IncreasesItemCount()
{
    // Arrange
    var inventory = new Inventory(maxSlots: 10);
    var sword = new Item("Sword", weight: 5f);

    // Act
    bool wasAdded = inventory.AddItem(sword);

    // Assert
    Assert.IsTrue(wasAdded);
    Assert.AreEqual(1, inventory.ItemCount);
}
```

# PlayMode Coroutine Tests

- ✅ Use `yield return null` to wait one frame.
- ✅ Use `yield return new WaitForSeconds(0.1f)` to wait real time (keep this minimal in tests).
- ✅ Use `yield return new WaitForFixedUpdate()` to wait for the next physics step.
- ❌ Avoid long `WaitForSeconds` calls — they slow down the test suite significantly.

```csharp
[UnityTest]
public IEnumerator Projectile_AfterFiring_MovesForward()
{
    // Arrange
    var go = new GameObject("Projectile");
    var projectile = go.AddComponent<Projectile>();
    var startPos = go.transform.position;

    // Act
    projectile.Fire(direction: Vector3.forward, speed: 10f);
    yield return new WaitForFixedUpdate();

    // Assert
    Assert.Greater(go.transform.position.z, startPos.z);
    Object.Destroy(go);
}
```

# Error and Log Assertion

- ✅ Use `LogAssert.Expect(LogType.Error, "expected message")` to assert that a specific error log is emitted.
- ✅ Call `LogAssert.Expect` **before** the code that triggers the log.
- ✅ Use `LogAssert.NoUnexpectedReceived()` at the end of a test to fail on unexpected logs.
- ❌ Never swallow Unity errors silently — unexpected error logs will fail the test by default in UTF.

```csharp
[Test]
public void Initialize_WithNullConfig_LogsError()
{
    // Arrange
    var system = new GameSystem();
    LogAssert.Expect(LogType.Error, "GameSystem: Config cannot be null.");

    // Act
    system.Initialize(config: null);

    // Assert — LogAssert.Expect verifies the error was emitted
}
```

# Mocking with Interfaces

- ✅ Design production code against interfaces so dependencies can be substituted in tests.
- ✅ Create lightweight manual fakes/stubs rather than using a full mocking framework when simple substitution is enough.
- ✅ Use NSubstitute or Moq if available in the project and the team is comfortable with the API.
- ❌ Never instantiate real services (network, file system, time) in unit tests — inject interfaces instead.

```csharp
// Production interface
public interface IScoreService
{
    void SubmitScore(int score);
    int GetHighScore();
}

// Manual stub for testing
public class FakeScoreService : IScoreService
{
    public int SubmittedScore { get; private set; }
    public int HighScoreToReturn { get; set; } = 0;

    public void SubmitScore(int score) => SubmittedScore = score;
    public int GetHighScore() => HighScoreToReturn;
}

// Test using the stub
[Test]
public void GameOver_SubmitsCurrentScore()
{
    // Arrange
    var fakeService = new FakeScoreService();
    var game = new GameSession(scoreService: fakeService);
    game.AddPoints(250);

    // Act
    game.TriggerGameOver();

    // Assert
    Assert.AreEqual(250, fakeService.SubmittedScore);
}
```

# Naming Conventions

- ✅ Test class name: `{ClassUnderTest}Tests` (e.g., `HealthSystemTests`, `PlayerControllerTests`).
- ✅ Test method name: `{MethodUnderTest}_{Scenario}_{ExpectedResult}`.
- ✅ Examples: `TakeDamage_BelowZero_ClampsToZero`, `AddItem_WhenFull_ReturnsFalse`.
- ❌ Avoid vague names like `Test1`, `CheckHealth`, or `VerifyInventory`.

# Testing a State Machine

```csharp
// Production state machine (simplified)
public class EnemyStateMachine
{
    public EnemyState CurrentState { get; private set; } = EnemyState.Idle;

    public void SetState(EnemyState newState)
    {
        CurrentState = newState;
    }

    public void Update(float distanceToPlayer)
    {
        switch (CurrentState)
        {
            case EnemyState.Idle:
                if (distanceToPlayer < 10f)
                    SetState(EnemyState.Chase);
                break;
            case EnemyState.Chase:
                if (distanceToPlayer > 15f)
                    SetState(EnemyState.Idle);
                break;
        }
    }
}

// Tests
[TestFixture]
public class EnemyStateMachineTests
{
    private EnemyStateMachine m_StateMachine;

    [SetUp]
    public void SetUp()
    {
        m_StateMachine = new EnemyStateMachine();
    }

    [Test]
    public void Update_WhenIdleAndPlayerNear_TransitionsToChase()
    {
        // Arrange — starts in Idle by default
        Assert.AreEqual(EnemyState.Idle, m_StateMachine.CurrentState);

        // Act
        m_StateMachine.Update(distanceToPlayer: 5f);

        // Assert
        Assert.AreEqual(EnemyState.Chase, m_StateMachine.CurrentState);
    }

    [Test]
    public void Update_WhenChasingAndPlayerFar_TransitionsToIdle()
    {
        // Arrange
        m_StateMachine.SetState(EnemyState.Chase);

        // Act
        m_StateMachine.Update(distanceToPlayer: 20f);

        // Assert
        Assert.AreEqual(EnemyState.Idle, m_StateMachine.CurrentState);
    }

    [Test]
    public void Update_WhenChasingAndPlayerClose_RemainsChasing()
    {
        // Arrange
        m_StateMachine.SetState(EnemyState.Chase);

        // Act
        m_StateMachine.Update(distanceToPlayer: 5f);

        // Assert
        Assert.AreEqual(EnemyState.Chase, m_StateMachine.CurrentState);
    }
}
```

# Code Coverage

- ✅ Enable code coverage via the Unity Test Runner with the `--coverage` flag or via **Edit > Preferences > Code Coverage**.
- ✅ Use the `[ExcludeFromCoverage]` attribute from `UnityEngine.TestTools.Coverage` on generated or trivial code.
- ✅ Aim for high coverage on logic-heavy, non-MonoBehaviour classes (calculators, state machines, services).
- ❌ Do not pursue 100% coverage at the expense of test quality — cover meaningful paths, not every getter.
- ℹ️ Coverage reports are generated as HTML in `CodeCoverage/` at the project root.
