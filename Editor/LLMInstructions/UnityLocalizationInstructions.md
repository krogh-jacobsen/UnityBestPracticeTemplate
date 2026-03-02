
# GitHub Copilot Instructions: Unity Localization

Use this cheat sheet for LLM completions. See the readme.md for the general rationale behind these guidelines.

Table of contents:
- [Unity Version-Specific Instructions](#unity-version-specific-instructions)
- [Core Concepts](#core-concepts)
- [LocalizedString](#localizedstring)
- [StringTable and StringTableEntry](#stringtable-and-stringtableentry)
- [Getting a Localized String at Runtime](#getting-a-localized-string-at-runtime)
- [Smart Strings](#smart-strings)
- [Plurals with Smart Strings](#plurals-with-smart-strings)
- [Locale Switching](#locale-switching)
- [LocalizeStringEvent Component](#localizestringevent-component)
- [Localizing Non-String Assets](#localizing-non-string-assets)
- [Avoiding Hardcoded Strings](#avoiding-hardcoded-strings)

# Unity Version-Specific Instructions

- ℹ️ This project uses Unity 6.3 and the **Localization** package 1.5+. Install it via Package Manager.
- ℹ️ Unity Localization 1.5 supports `LocalizedString.GetLocalizedStringAsync()` returning an `AsyncOperationHandle<string>` — prefer this over the synchronous `GetLocalizedString()` which blocks.
- ℹ️ Smart Strings use the `SmartFormat` library — see the package documentation for the full formatter list.
- ℹ️ `LocalizationSettings` is the central singleton — do not create your own locale management layer on top of it.

# Core Concepts

- ✅ All user-visible strings must come from a `StringTable` — never hardcode display text.
- ✅ Assets that vary by locale (sprites, audio clips) must use `LocalizedAsset<T>` — never switch them with `if (locale == "en")` logic.
- ✅ Subscribe to `LocalizationSettings.SelectedLocaleChanged` to react to locale changes at runtime.
- ❌ Never concatenate localized strings with other strings — use Smart String placeholders instead.
- ❌ Never assume locale order or index is stable — always compare by `Locale.Identifier.Code`.

# LocalizedString

- ✅ Serialize `LocalizedString` fields in MonoBehaviours and ScriptableObjects — wire them up in the Inspector.
- ✅ `LocalizedString` stores both the table name and the entry key — it is fully self-contained.
- ✅ Subscribe to `LocalizedString.StringChanged` to receive automatic updates when the locale changes.
- ❌ Never call `GetLocalizedString()` on a `LocalizedString` inside `Update` — it is a blocking synchronous call.

```csharp
public class UILabel : MonoBehaviour
{
    [SerializeField] private LocalizedString m_LabelText;
    [SerializeField] private TMP_Text m_TextField;

    private void OnEnable()
    {
        // StringChanged fires immediately with the current locale value, then again on locale change
        m_LabelText.StringChanged += HandleStringChanged;
    }

    private void OnDisable()
    {
        m_LabelText.StringChanged -= HandleStringChanged;
    }

    private void HandleStringChanged(string localizedValue)
    {
        m_TextField.text = localizedValue;
    }
}
```

# StringTable and StringTableEntry

- ℹ️ A `StringTable` is an asset that holds key-value pairs for one locale (e.g. `UI_en` for English, `UI_fr` for French).
- ℹ️ All locales share the same **table collection name** and **entry keys** — only the values differ.
- ✅ Name tables by domain: `UI`, `Dialogue`, `Items`, `Errors`. Avoid a single giant table.
- ✅ Keep entry keys in a static constants class to prevent typos and enable refactoring.
- ❌ Never use raw string literals as table keys in code — always reference a constant.

```csharp
// Centralised table key constants
public static class UIStringKeys
{
    public const string TableName = "UI";

    public const string StartButton = "ui_btn_start";
    public const string SettingsTitle = "ui_title_settings";
    public const string HealthLabel = "ui_label_health";
    public const string ScoreFormat = "ui_format_score"; // "Score: {score}"
}
```

# Getting a Localized String at Runtime

- ✅ Use `LocalizedString.GetLocalizedStringAsync()` for async retrieval.
- ✅ Use `LocalizationSettings.StringDatabase.GetLocalizedStringAsync(table, key)` for direct database access.
- ✅ Await the `AsyncOperationHandle<string>.Task` in an async method or use the callback overload.
- ❌ Avoid `LocalizationSettings.StringDatabase.GetLocalizedString(table, key)` (synchronous) in production — it is a blocking call.

```csharp
public class ToastNotification : MonoBehaviour
{
    [SerializeField] private TMP_Text m_MessageText;

    public async void ShowMessage(string tableKey)
    {
        var handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(
            UIStringKeys.TableName, tableKey);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
            m_MessageText.text = handle.Result;
        else
            Debug.LogError($"[Localization] Failed to load key: {tableKey}");
    }
}
```

# Smart Strings

- ℹ️ Smart Strings allow embedding variables inside localized text using `{variableName}` syntax.
- ✅ Use `LocalizedString.Arguments` to pass values into Smart String placeholders.
- ✅ Smart Strings are evaluated at retrieval time — arguments are formatted according to the current locale's culture (number formats, date formats).
- ❌ Do not use `string.Format` or `$""` interpolation on localized strings — use Smart String placeholders.

```csharp
// StringTable entry for "ui_format_score":
//   English: "Score: {score:N0}"
//   German:  "Punkte: {score:N0}"

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private LocalizedString m_ScoreFormat;
    [SerializeField] private TMP_Text m_ScoreText;

    private int m_CurrentScore;

    private void OnEnable()
    {
        m_ScoreFormat.StringChanged += HandleStringChanged;
    }

    private void OnDisable()
    {
        m_ScoreFormat.StringChanged -= HandleStringChanged;
    }

    public void SetScore(int score)
    {
        m_CurrentScore = score;

        // Pass a named argument matching the {score} placeholder
        m_ScoreFormat.Arguments = new object[]
        {
            new { score = m_CurrentScore }
        };

        // Refresh — StringChanged will fire with the formatted result
        m_ScoreFormat.RefreshString();
    }

    private void HandleStringChanged(string localizedValue)
    {
        m_ScoreText.text = localizedValue;
    }
}
```

# Plurals with Smart Strings

- ✅ Use the `plural` formatter for count-sensitive text: `{count:plural:one item|{count} items}`.
- ✅ The plural rules follow CLDR rules per locale — no custom plural logic needed in code.
- ℹ️ The general syntax is `{count:plural:zero|one|two|few|many|other}` — not all locales use all categories.

```csharp
// StringTable entry for "ui_item_count":
//   English: "You have {count:plural:one item|{count} items}."
//   Russian: "У вас {count:plural:{count} предмет|{count} предмета|{count} предметов}."

public class ItemCountLabel : MonoBehaviour
{
    [SerializeField] private LocalizedString m_ItemCountString;
    [SerializeField] private TMP_Text m_Label;

    private void OnEnable()
    {
        m_ItemCountString.StringChanged += value => m_Label.text = value;
    }

    private void OnDisable()
    {
        m_ItemCountString.StringChanged -= value => m_Label.text = value;
    }

    public void UpdateCount(int count)
    {
        m_ItemCountString.Arguments = new object[] { new { count } };
        m_ItemCountString.RefreshString();
    }
}
```

# Locale Switching

- ✅ Use `LocalizationSettings.SelectedLocale` to get or set the active locale.
- ✅ Assign a `Locale` asset (found in the Localization Tables window) — not a raw `LocaleIdentifier`.
- ✅ Subscribe to `LocalizationSettings.SelectedLocaleChanged` in `OnEnable` and unsubscribe in `OnDisable`.
- ❌ Never switch locale by string comparison — always compare `Locale` objects or `LocaleIdentifier.Code`.

```csharp
public class LocaleSwitcher : MonoBehaviour
{
    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;
    }

    private void HandleLocaleChanged(Locale newLocale)
    {
        Debug.Log($"[Locale] Switched to: {newLocale.Identifier.Code}");
        // All LocalizedString subscribers will automatically receive updated values
    }

    public void SetLocale(string localeCode)
    {
        var available = LocalizationSettings.AvailableLocales.Locales;
        var target = available.Find(l => l.Identifier.Code == localeCode);

        if (target != null)
            LocalizationSettings.SelectedLocale = target;
        else
            Debug.LogWarning($"[Locale] Locale not found: {localeCode}");
    }
}
```

# LocalizeStringEvent Component

- ✅ Use the `LocalizeStringEvent` component (from `UnityEngine.Localization.Components`) to localize UI text without writing code.
- ✅ Wire the `OnUpdateString` UnityEvent on `LocalizeStringEvent` to the `TMP_Text.text` setter via the Inspector.
- ✅ Use the Smart String arguments list on `LocalizeStringEvent` to supply values to `{placeholders}` in the Inspector.
- ❌ Do not add both a `LocalizeStringEvent` and manual `LocalizedString.StringChanged` subscriptions to the same label — they will conflict.

# Localizing Non-String Assets

- ✅ Use `LocalizedAsset<T>` for assets that differ by locale: `LocalizedSprite`, `LocalizedAudioClip`, `LocalizedTexture`.
- ✅ Register a custom `LocalizedAsset<T>` for any serializable asset type via `[LocalizedAssetAttribute]`.
- ✅ Use `LocalizedAsset<T>.AssetChanged` for event-driven updates, same as `LocalizedString.StringChanged`.
- ❌ Never switch sprites or audio clips with locale-code comparisons in code — use `LocalizedAsset<T>`.

```csharp
// Localizing a sprite (e.g. flag icons, locale-specific artwork)
public class LocalizedSpriteDisplay : MonoBehaviour
{
    [SerializeField] private LocalizedSprite m_LocalizedSprite;
    [SerializeField] private Image m_Image;

    private void OnEnable()
    {
        m_LocalizedSprite.AssetChanged += HandleSpriteChanged;
    }

    private void OnDisable()
    {
        m_LocalizedSprite.AssetChanged -= HandleSpriteChanged;
    }

    private void HandleSpriteChanged(Sprite sprite)
    {
        m_Image.sprite = sprite;
    }
}
```

# Avoiding Hardcoded Strings

- ❌ Never do: `myText.text = "Start Game";`
- ❌ Never do: `myText.text = locale == "fr" ? "Démarrer" : "Start Game";`
- ✅ Always use a `LocalizedString` field or `LocalizeStringEvent` component.
- ✅ For error messages shown to developers only (not players), plain strings are acceptable — distinguish between developer-facing and player-facing text.

```csharp
// AVOID
public class BadMenuButton : MonoBehaviour
{
    private void Start()
    {
        GetComponent<TMP_Text>().text = "Start Game"; // hardcoded — breaks localization
    }
}

// PREFER
public class GoodMenuButton : MonoBehaviour
{
    [SerializeField] private LocalizedString m_ButtonLabel;
    private TMP_Text m_Text;

    private void Awake()
    {
        m_Text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        m_ButtonLabel.StringChanged += value => m_Text.text = value;
    }

    private void OnDisable()
    {
        m_ButtonLabel.StringChanged -= value => m_Text.text = value;
    }
}
```
