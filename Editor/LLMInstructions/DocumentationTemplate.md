# Documentation Template

This template guides Claude Code when generating README.md files using `/document`.

---

## Structure

```markdown
# {Feature Name}: {Specific Topic} Tutorial

Brief 1-2 sentence description of what this feature does and its primary use case.

## Overview

Explain what this is in context. Compare to alternatives or previous approaches if relevant. Keep to 2-3 sentences—just enough context for the reader to understand where this fits.

## Setup

Minimal code or configuration to get started:

```csharp
using UnityEngine.SomeNamespace;
```

## {Core Concept 1}

Explanation of the first key concept with a practical code example:

```csharp
private void ExampleMethod()
{
    // Commented explanation of what this does
    SomeClass.DoThing();
}
```

## {Core Concept 2}

### {Subsection if needed}

More focused explanation with code:

```csharp
private void AnotherExample()
{
    // Show the specific pattern
}
```

### {Another Subsection}

Additional pattern or variation.

## {Core Concept 3}

Continue with additional concepts as needed. Each section should be self-contained with its own code example.

## Key Takeaways

| Feature | Value |
|---------|-------|
| {Attribute 1} | {Description} |
| {Attribute 2} | {Description} |
| {Attribute 3} | {Description} |
| Best for | {Use case} |

## When to Use

- ✅ {Good use case 1}
- ✅ {Good use case 2}
- ❌ {Anti-pattern 1}
- ❌ {Anti-pattern 2}

Brief sentence recommending alternatives for the anti-patterns if applicable.

## Learn More
Brief sentence about where to find more information, followed by links:
[Unity Documentation Link](https://docs.unity3d.com/...)
[Unity Learn Link](https://learn.unity.com/...)
```

---

## Style Guidelines

### Tone
- Tutorial-style: teach through examples
- Assume reader is intermediate Unity developer
- Explain "why" briefly, focus on "how"
- Be concise—every sentence should add value

### Section Guidelines

| Section | Purpose | Length |
|---------|---------|--------|
| Title | Feature + specific topic | 5-8 words |
| Opening | Quick context | 1-2 sentences |
| Overview | Where this fits, alternatives | 2-3 sentences |
| Setup | Bare minimum to begin | 3-5 lines of code |
| Core Concepts | Teach through code | 1 paragraph + code block each |
| Key Takeaways | Quick reference table | 4-6 rows |
| When to Use | Decision guidance | 2-4 pros, 2-4 cons |
| Learn More | External resources | 1-3 links |

### Code Examples
- Keep examples minimal and focused (10-20 lines max)
- Include comments only for non-obvious lines
- Use realistic variable names with `m_` prefix for private fields
- Show one concept per code block
- Prefer `private void MethodName()` pattern for Unity methods

### Formatting
- Use sentence case for headings
- Code blocks with `csharp` language hint
- Tables for comparisons and summaries
- Checkmarks (✅ ❌) for quick pros/cons lists
- No emojis elsewhere in the document

### What to Include
- Always: Overview, Setup, at least 2 Core Concepts, Key Takeaways
- For patterns: When to Use section
- For APIs: Code examples for each main method
- For systems: How components interact

### What to Avoid
- Long explanations without code
- Multiple concepts in one code block
- Implementation details that don't help understanding
- Repeating information from code comments
- Version history or TODO items

---

## Example Invocations

```
/document
```
Generates README using this template structure.

```
/document focus on the data binding setup
```
Emphasizes specific aspect in Core Concepts.

```
/document brief
```
Generates Overview, Setup, and Key Takeaways only.
